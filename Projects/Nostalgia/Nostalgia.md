# Nostalgia 작업 소스 코드

> 이 문서는 **"Nostalgia"** 프로젝트에서 제가 작업한 주요 내용에 대해 정리하고 설명합니다.

<br />

## 유저 세팅 기능 및 UI 구현

> 유저의 세팅을 UI에서 조절하고, 게임을 다시 실행했을 때 자동으로 값을 불러올 수 있도록 Scriptable Object를 사용하여 만들었습니다.

<details>
    <summary>
        SettingsSO.cs
    </summary>
    
```csharp
public abstract class SettingsSO : ScriptableObject
{
    public virtual void Load()
    {
        string path = GetSavePath();
        
        if (!File.Exists(path))
        {
            Save();
            return;
        }

        string json = File.ReadAllText(path);
        JsonUtility.FromJsonOverwrite(json, this);
    }

    protected void Save()
    {
        string path = GetSavePath();
        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(path, json);
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, $"{name}.json");
    }
}
```

</details>

🔍 [유저 세팅 기능 및 UI 구현 코드 더 보기](./User%20Settings/)

<br />

## 몹 코드 리펙토링

> 기존의 코루틴 방식으로 작동하던 몹 코드를 Fusion에서 제공하는 FSM을 사용하여 리펙토링 하였습니다.

<details>
    <summary>
        SadAI.cs
    </summary>

```csharp
[RequireComponent(typeof(StateMachineController))]
public class SadAI : BaseMob, IStateMachineOwner
{
    private const float MIN_CRYING_INTERVAL = 17f;
    private const float MAX_CRYING_INTERVAL = 19f;
    
    private StateMachine<StateBehaviour> m_sadAI;

    [Header("State")] 
    [SerializeField] private SadIdleBehaviour m_idleState;
    [SerializeField] private SadChaseBehaviour m_chaseState;
    [SerializeField] private SadAttackBehaviour m_attackState;

    [Header("Events")] 
    [SerializeField] private AttackEvent m_attackEvent;

    [Header("Effect")] 
    [SerializeField] private GameObject m_leftFoot;
    [SerializeField] private GameObject m_rightFoot;
    [SerializeField] private GameObject particleObject;
    
    
    private bool m_bIsPlayerFind = false;

    public override void Spawned()
    {
        base.Spawned();

        StartCoroutine(Cry());
        particleObject.SetActive(false);
        
        // 아빠 플레이어에서만 플레이
        if (Runner.LocalPlayer == GameManager.Instance.FatherPlayerRef)
        {
            StartCoroutine(FootParticlePlayCoroutine());
            particleObject.SetActive(true);
        }
    }

    void IStateMachineOwner.CollectStateMachines(List<IStateMachine> stateMachines)
    {
        m_sadAI = new StateMachine<StateBehaviour>(
            "Sad AI", 
            m_idleState, m_chaseState, m_attackState
        );
        
        m_sadAI.SetDefaultState(m_idleState.StateId);
        stateMachines.Add(m_sadAI);
    }
    
    public override void FixedUpdateNetwork()
    {
        // 다른 상태 조건이 없다면 기본적으로 Idle로 돌아감
        m_bIsPlayerFind = Observe();

        if (m_attackEvent.attackFlag)
        {
            m_sadAI.ForceActivateState(m_attackState);
            return;
        }

        if (m_bIsPlayerFind)
        {
            m_chaseState.ResetInvisibilityDuration();
            m_sadAI.ForceActivateState(m_chaseState);
        }
    }

    private IEnumerator Cry() 
    {
        while(true) 
        {
            float waitTime = Random.Range(MIN_CRYING_INTERVAL, MAX_CRYING_INTERVAL);
            yield return new WaitForSeconds(waitTime);
            SoundManager.Instance.SFX_Play_rpc("sadCry1", mobNetworkObject);
        }
    }

    private IEnumerator FootParticlePlayCoroutine()
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y - 1.0f, transform.position.z);
        Vector3 leftPosition = pos  - transform.right * 0.8f;
        Vector3 rightPosition = pos + transform.right * 0.8f;

        GameObject  leftFootObject = Instantiate( m_leftFoot,  leftPosition, transform.rotation);
        GameObject rightFootObject = Instantiate(m_rightFoot, rightPosition, transform.rotation);

        float coolTime = 2.5f;
        while (true)
        {
            coolTime = CurrentState switch
            {
                MobState.Idle  => 2.5f,
                MobState.Chase => 1.2f,
                _              => coolTime
            };

            Destroy(leftFootObject);
            
            pos = new Vector3(transform.position.x, transform.position.y - 0.97f, transform.position.z);
            leftPosition = pos - transform.right * 0.8f;
            leftFootObject = Instantiate(m_leftFoot, leftPosition, transform.rotation);
            leftFootObject.SetActive(true);
            
            yield return new WaitForSeconds(coolTime);

            Destroy(rightFootObject);
            
            pos = new Vector3(transform.position.x, transform.position.y - 0.97f, transform.position.z);
            rightPosition = pos + transform.right * 0.8f;
            rightFootObject = Instantiate(m_rightFoot, rightPosition, transform.rotation);
            rightFootObject.SetActive(true);
            
            yield return new WaitForSeconds(coolTime);
        }
    }
}

```
    
</details>

<details>
    <summary>
        ModSO.cs
    </summary>

```csharp
[CreateAssetMenu(fileName = "SO_", menuName = "Scriptable Object/Mob", order = 0)]
public class MobSO : ScriptableObject
{
    [Header("State Speeds")] 
    public float[] StateSpeeds = { 2f, 3f, 5f };
    
    [Header("Observing")]
    public float ViewRadius = 1.5f;
    public float ViewAngle = 60f;
    
    [Header("Attack")]
    public float AttackCoolTime = 0f;
    public int AttackDamage = 100;
    
    [Header("Layer Mask")]
    public LayerMask PlayerLayerMask;
    public LayerMask ObstacleLayerMask;
}

```

</details>

🔍 [몹 리펙토링 코드 더 보기](./Mob/)

<br />

## 사용 아이템 

> 기존에는 필드에서 습득할 수 있는 아이템이 클리어 목표 아이템인 일기장 밖에 없었으나, 게임이 루즈해지는 것 같다는 의견이 있었습니다. 따라서 습득하여 사용할 수 있는 아이템을 구현하였습니다.

<details>
    <summary>
        ConsumableItem.cs
    </summary>

```csharp
[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform), typeof(BoxCollider))]
public class ConsumableItem : NetworkBehaviour, IInteractable
{
    [Header("Item Scriptable Object")]
    [SerializeField] private ConsumableItemSO m_itemSO;

    [Header("Interactable Prompt Data")] 
    [SerializeField] private InteractPromptData m_interactPromptData;

    public void OnInteract(NetworkObject playerObject)
    {
        Player player = playerObject.GetComponent<Player>();

        Debug.LogWarning("Consumable Item OnInteract");
       
        // 플레이어의 현재 인벤토리 용량 확인
        // 추가 불가능하면 return
        // 가능하면 AddItem
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        if(playerInventory != null) {
            if(playerInventory.CanAddItem(m_itemSO)) {
                playerInventory.AddItem(m_itemSO);
                DespawnRpc();
            }
            else {
                Debug.Log("아이템을 추가할 수 없습니다.");
                return;
            }
        }

    }

    public InteractPromptData GetInteractPromptData()
    {
        return m_interactPromptData;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void DespawnRpc()
    {
        Runner.Despawn(GetComponent<NetworkObject>());
    }
}

```
    
</details>

<details>
    <summary>
        ConsumableItemSO.cs
    </summary>

```csharp
public abstract class ConsumableItemSO : ScriptableObject
{
    [Header("Default Item Config")]
    [SerializeField] protected LocalizedString itemName;
    public string ItemName => itemName.GetLocalizedString();

    [SerializeField] protected LocalizedString description;
    public string Description => description.GetLocalizedString();

    [SerializeField] protected Sprite icon;
    public Sprite Icon => icon;

    public abstract void Use(Player usingPlayer);
}

```
    
</details>

🔍 [사용 아이템 코드 더 보기](./Item/)

<br />

## 일기장 시스템 리펙토링

> 일기장 관리, UI 로직까지 모두 하나의 스크립트에서 작성되어 있던 것을 기능별로 분리하여 이해하기 쉽도록 하였습니다.

🔍 [일기장 코드 보기]()

<br />

---

🏠 [메인 페이지로 돌아가기](../../README.md)
