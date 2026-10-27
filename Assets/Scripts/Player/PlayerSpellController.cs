using UnityEngine;

// Đảm bảo phải có Mana Component đi kèm
[RequireComponent(typeof(PlayerManaComponent))]
public class PlayerSpellController : MonoBehaviour
{
    private PlayerInputHandler inputHandler;
    [Header("Spell Slots (Kéo Prefab Phép Thuật vào đây)")]
    public GameObject[] equippedSpellPrefabs = new GameObject[5]; 
    
    // Mảng lưu đệm Data để KHÔNG gọi GetComponent trong Update
    private D_Spell[] cachedSpellData = new D_Spell[5]; 
    private float[] cooldownTimers = new float[5];
    public bool[] toggleStates = new bool[5];
    private GameObject[] activeToggleVFX = new GameObject[5];
    private PlayerManaComponent playerMana;
    public int unlockedSlotCount { get; private set; } = 2;
    private bool isWaitingToReplace = false;
    private GameObject pendingSpellPrefab;
    private float[] toggleFireTimers = new float[5];
    

    private void Awake()
    {
        // Liên kết nội bộ thay vì dùng PlayerController.Instance
        playerMana = GetComponent<PlayerManaComponent>();
        inputHandler = GetComponent<PlayerInputHandler>();
        //if (inputHandler == null) inputHandler = GetComponentInParent<PlayerInputHandler>();
    }

    private void Start()
    {
        // Lưu đệm toàn bộ data lúc bắt đầu
        for (int i = 0; i < equippedSpellPrefabs.Length; i++)
        {
            CacheSpellData(i, equippedSpellPrefabs[i]);
        }
        InitializeUI();
    }

    private void Update()
    {
        // COOLDOWN
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            // Nếu là phép Toggle và ĐANG BẬT: Giữ UI màu Xanh, bỏ qua trừ Cooldown
            if (cachedSpellData[i] != null && cachedSpellData[i].isManualToggle && toggleStates[i])
            {
                if (UIManager.Instance != null) UIManager.Instance.UpdateCooldownUI(i + 1, 1f, 1f, true);
                continue; 
            }

            // Nếu đang trong thời gian hồi chiêu
            if (cooldownTimers[i] > 0)
            {
                cooldownTimers[i] -= Time.deltaTime;
                if (UIManager.Instance != null && cachedSpellData[i] != null)
                {
                    // Truyền false để hiện màu Đỏ (hồi chiêu bình thường)
                    UIManager.Instance.UpdateCooldownUI(i + 1, cooldownTimers[i], cachedSpellData[i].cooldown, false);
                }
            }
        }
        // MANA
        // MANA & AUTO FIRE CHO TOGGLE
        for (int i = 0; i < toggleStates.Length; i++)
        {
            if (toggleStates[i] && cachedSpellData[i] != null)
            {
                D_Spell data = cachedSpellData[i];

                // 1. Trừ mana dựa theo ManaCost mỗi giây (Không dùng Effect nữa)
                float manaDrainPerSec = data.manaCost;
                if (manaDrainPerSec > 0)
                {
                    playerMana.RestoreMana(-manaDrainPerSec * Time.deltaTime);
                }

                // Hết Mana -> Tự tắt
                if (playerMana.currentMana <= 0)
                {
                    Debug.Log($"Hết Mana! Tự động tắt chiêu ở slot {i+1}");
                    ToggleSpell(i, gameObject);
                    continue;
                }

                // 2. KHAI HỎA TỰ ĐỘNG
                bool isSpamToggle = !data.isSelfTargeted && !data.hasAura; 
                
                if (isSpamToggle)
                {
                    toggleFireTimers[i] -= Time.deltaTime;
                    if (toggleFireTimers[i] <= 0)
                    {
                        toggleFireTimers[i] = data.cooldown > 0 ? data.cooldown : 1f; 
                        AutoFireToggleSpell(i, data);
                    }
                }
            }
        }
        // REPLACE
        if (isWaitingToReplace && pendingSpellPrefab != null && inputHandler != null)
        {
            int slotToReplace = -1;

            // Kiểm tra Input từ hệ thống mới dựa trên số ô đã mở khóa
            if (inputHandler.Spell1Input && unlockedSlotCount > 0) slotToReplace = 0;
            else if (inputHandler.Spell2Input && unlockedSlotCount > 1) slotToReplace = 1;
            else if (inputHandler.Spell3Input && unlockedSlotCount > 2) slotToReplace = 2;
            else if (inputHandler.Spell4Input && unlockedSlotCount > 3) slotToReplace = 3;
            else if (inputHandler.Spell5Input && unlockedSlotCount > 4) slotToReplace = 4;

            if (slotToReplace != -1)
            {
                // 1. Tắt tín hiệu Input ngay lập tức để không bị tự động xuất chiêu khi game hết Pause
                inputHandler.UseSpellInputByIndex(slotToReplace);

                // 2. Lắp đè phép
                EquipSpell(slotToReplace, pendingSpellPrefab);
                Debug.Log($"Đã thay thế ô số {slotToReplace + 1} bằng phép mới!");
                
                // 3. Xóa trạng thái chờ
                isWaitingToReplace = false;
                pendingSpellPrefab = null;

                // 4. Đóng bảng và gọi lần lên cấp tiếp theo (nếu nợ)
                UIManager.Instance.ShowLevelUpPanel(false);
                if (TryGetComponent(out PlayerLevelComponent levelComp))
                {
                    levelComp.CheckAndShowLevelUpPanel();
                }
            }
        }
    }

    // Hàm tiện ích để nạp Data vào mảng đệm
    private void CacheSpellData(int index, GameObject prefab)
    {
        if (prefab != null && prefab.TryGetComponent(out SpellController spellCtrl))
        {
            // SỬ DỤNG baseSpellData ĐÚNG THEO KIẾN TRÚC MỚI
            cachedSpellData[index] = spellCtrl.baseSpellData; 
        }
        else
        {
            cachedSpellData[index] = null;
        }
    }
    public void ToggleSpell(int slotIndex, GameObject playerObj)
    {
        D_Spell data = cachedSpellData[slotIndex];
        if (data == null || !data.isManualToggle) return;

        // Đảo ngược trạng thái
        toggleStates[slotIndex] = !toggleStates[slotIndex];

        if (toggleStates[slotIndex])
        {
            if (UIManager.Instance != null) UIManager.Instance.UpdateCooldownUI(slotIndex + 1, 1f, 1f, true);
            
            // XỬ LÝ ĐẺ ĐÚNG 1 LẦN CHO BUFF TỰ THÂN HOẶC AURA
            if (data.isSelfTargeted || data.hasAura) 
            {
                Vector3 spawnPos = playerObj != null ? playerObj.transform.position : transform.position;
                Transform parent = null;

                if (data.isSelfTargeted && playerObj != null) 
                {
                    parent = playerObj.transform;
                }
                else if (data.isSpawnAtMouse)
                {
                    Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(inputHandler.MousePosition);
                    Vector2 origin = PlayerController.Instance != null && PlayerController.Instance.attackPosition != null
                        ? (Vector2)PlayerController.Instance.attackPosition.position
                        : (Vector2)transform.position;

                    float distanceToMouse = Vector2.Distance(origin, mouseWorldPos);
                    Vector2 shootDir = (mouseWorldPos - origin).normalized;

                    spawnPos = distanceToMouse > data.maxCastRange
                        ? (Vector3)(origin + shootDir * data.maxCastRange)
                        : (Vector3)mouseWorldPos;
                    spawnPos.z = 0f;
                }

                GameObject auraObj = ObjectPoolManager.Instance != null
                    ? ObjectPoolManager.Instance.GetFromPool(equippedSpellPrefabs[slotIndex], spawnPos, Quaternion.identity)
                    : Instantiate(equippedSpellPrefabs[slotIndex], spawnPos, Quaternion.identity);

                if (parent != null)
                {
                    auraObj.transform.SetParent(parent);
                    auraObj.transform.localPosition = Vector3.zero;
                }

                activeToggleVFX[slotIndex] = auraObj; 

                if (auraObj.TryGetComponent(out SpellController activeCtrl))
                {
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(inputHandler.MousePosition);
                    Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
                    activeCtrl.Initialize(dir);
                }
            }
        }
        else
        {
            // TẮT:
            TriggerCooldown(slotIndex);
            if (UIManager.Instance != null) UIManager.Instance.UpdateCooldownUI(slotIndex + 1, cooldownTimers[slotIndex], 1f, false);

            // 1. Tắt object đã lưu trong mảng activeToggleVFX
            if (activeToggleVFX[slotIndex] != null) 
            {
                if (activeToggleVFX[slotIndex].TryGetComponent(out SpellController ctrl))
                {
                    ctrl.Despawn(); 
                }
                else
                {
                    Destroy(activeToggleVFX[slotIndex]); 
                }
                activeToggleVFX[slotIndex] = null; 
            }

            if (!data.isSelfTargeted)
            {
                SpellController[] activeSpells = FindObjectsByType<SpellController>(FindObjectsInactive.Exclude);
                foreach (var spell in activeSpells)
                {
                    if (spell.baseSpellData != null && spell.baseSpellData.name == data.name) 
                    {
                        spell.Despawn(); 
                    }
                }
            }
        }
    }
    private void AutoFireToggleSpell(int slotIndex, D_Spell data)
    {
        // Lấy hướng ngắm từ chuột
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(inputHandler.MousePosition);
        Vector2 shootDirection = (mouseWorldPos - (Vector2)PlayerController.Instance.attackPosition.position).normalized;

        int count = data.spawnCount > 0 ? data.spawnCount : 1;
        
        Vector3 baseSpawnPos = PlayerController.Instance.attackPosition.position;
        if (data.isSpawnAtMouse) baseSpawnPos = mouseWorldPos;

        for (int i = 0; i < count; i++)
        {
            Vector3 finalSpawnPos = baseSpawnPos;
            Vector2 castDir = data.isSpawnAtMouse ? Vector2.zero : shootDirection;

            // Tính toán chùm đạn Spread (Nếu cần - copy logic từ PlayerCastingSpellState)
            if (!data.isSpawnAtMouse && data.isProjectile && count > 1 && data.multiShotPattern == MultiShotPattern.Spread)
            {
                float startAngle = -data.multiShotAngle / 2f;
                float step = count > 1 ? data.multiShotAngle / (count - 1) : 0;
                castDir = Quaternion.Euler(0, 0, startAngle + (step * i)) * shootDirection;
            }

            GameObject spellObj = ObjectPoolManager.Instance.GetFromPool(equippedSpellPrefabs[slotIndex], finalSpawnPos, Quaternion.identity);
            if (spellObj.TryGetComponent(out SpellController spellScript))
            {
                spellScript.Initialize(castDir, i, count, shootDirection);
            }   
        }
    }
    public void InitializeUI()
    {
        for (int i = 0; i < cachedSpellData.Length; i++)
        {
            if (UIManager.Instance != null)
            {
                Sprite icon = cachedSpellData[i] != null ? cachedSpellData[i].spellIcon : null;
                UIManager.Instance.SetSpellIcon(i, icon, i < unlockedSlotCount);
            }
        }
    }
    public void StartReplaceMode(GameObject newSpell)
    {
        isWaitingToReplace = true;
        pendingSpellPrefab = newSpell;
        UIManager.Instance.ChangeSText("Press 1, 2, 3... corresponding to the spell slot to replace");
    }
    public void CancelReplaceMode()
    {
        isWaitingToReplace = false;
        pendingSpellPrefab = null;
    }
    public void UnlockSlot(int slotIndex)
    {
        if (slotIndex < equippedSpellPrefabs.Length && slotIndex >= unlockedSlotCount)
        {
            unlockedSlotCount = slotIndex + 1;
            if (UIManager.Instance != null) UIManager.Instance.SetSpellIcon(slotIndex, null, true);
        }
    }
    public void EquipSpell(int slotIndex, GameObject spellPrefab)
    {
        if (slotIndex < 0 || slotIndex >= equippedSpellPrefabs.Length) return;
        if (slotIndex >= unlockedSlotCount) 
        {
            Debug.LogWarning("Ô phép này chưa được mở khóa!");
            return; 
        }
        equippedSpellPrefabs[slotIndex] = spellPrefab;
        
        // Cập nhật mảng đệm ngay khi thay đồ
        CacheSpellData(slotIndex, spellPrefab); 

        if (UIManager.Instance != null)
        {
            Sprite icon = cachedSpellData[slotIndex] != null ? cachedSpellData[slotIndex].spellIcon : null;
            UIManager.Instance.SetSpellIcon(slotIndex, icon, true);
        }
    }

    public bool CanCastSpell(int slotIndex)
    {
        if (slotIndex >= unlockedSlotCount) return false;
        D_Spell data = cachedSpellData[slotIndex];
        if (data == null) return false;

        if (data.isManualToggle && toggleStates[slotIndex]) return true;
        
        if (cooldownTimers[slotIndex] > 0) return false;
        if (playerMana.currentMana < data.manaCost) return false;

        return true;
    }

    public void TriggerCooldown(int slotIndex)
    {
        D_Spell data = cachedSpellData[slotIndex];
        if (data != null)
        {
            cooldownTimers[slotIndex] = data.cooldown;
        }
    }
}