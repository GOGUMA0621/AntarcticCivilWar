using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Firestore;
using System.Threading.Tasks;


//아이템 정보를 가진 클래스임
public class ItemDB
{
    public int price;
    public int cooltime;
    public string name_kr;
    public string name;
    public string type;
    public string ability_type;
    public ItemRarity rarity;
    public string effect;
    public string des;
    public List<float> base_effect = new();
    public List<float> stack_effect = new();
    public List<string> applied_debuff = new();
}

// 유닛 정보를 가진 클래스임
public class UnitDB
{   
    public int tier;
    public int mana;
    public int range;
    public int spawn_Stage;
    public float attack_Speed;
    public float speed;
    public string name_kr;
    public string name;
    public string type;
    public string attack_Type;
    public List<string> synergy = new();
    public List<int> hp = new();
    public List<int> attack = new();
}

// !!!!!!!!!!사용할 클래스에서 await FirebaseManager.ItemLoadData(); 또는 UnitLoadData();해주기!!!!!!!
public static class FirebaseManager
{
    private static FirebaseFirestore firestore = FirebaseFirestore.DefaultInstance;

    public static Dictionary<int, ItemDB> items = new Dictionary<int, ItemDB>();
    public static Dictionary<int, UnitDB> units = new Dictionary<int, UnitDB>();

    public static bool isLoaded => items != null && units != null;

    // 파이어베이스 db에 저장되어있는 정보들을 불러오는 과정임. 사용방식은 items[문서ID(아이템 No.)].value;
    public static async Task ItemLoadData()
    {
        QuerySnapshot itemDataLoad = await firestore.Collection("items").GetSnapshotAsync();
        try
        {
            foreach (DocumentSnapshot doc in itemDataLoad.Documents)
            {
                Dictionary<string, object> data = doc.ToDictionary();

                int itemId = int.Parse(doc.Id); // << 문서ID는 저장될때 문자열로 저장된다네요 그래서 Parse함수 사용

                ItemDB item = new ItemDB
                {
                    // Int.Parse()를 사용할 수 도있지만 파베에서 가져온 데이터는 object형식이라네요             
                    name_kr = data["name_kr"].ToString(),
                    name = data["name"].ToString(),
                    type = data["type"].ToString(),
                    ability_type = data["ability_type"].ToString(),
                    rarity = (ItemRarity)Enum.Parse(typeof(ItemRarity), (data["rarity"].ToString())),
                    effect = data["effect"].ToString(),

                    des = data.ContainsKey("des") ? data["des"].ToString() : "",

                    // 존재 여부에 따라 처리함
                    price = data.ContainsKey("price") ? Convert.ToInt32(data["price"]) : 0,
                    cooltime = data.ContainsKey("cooltime") ? Convert.ToInt32(data["cooltime"]) : 0,

                    applied_debuff = data.ContainsKey("applied_debuff") switch
                    {
                        true when data["applied_debuff"] is List<object> list =>
                            list.ConvertAll(obj => obj.ToString()),

                        true when data["applied_debuff"] is string str =>
                            new List<string> { str },

                        _ => new List<string>()
                    },

                    base_effect = data.ContainsKey("base_effect") switch
                    {
                        true when data["base_effect"] is List<object> list =>
                            list.ConvertAll(obj =>
                            {
                                try { return Convert.ToSingle(obj); }
                                catch
                                {
                                    Debug.LogWarning($"base_effect 리스트 내부 변환 실패: {obj} ({obj?.GetType()})");
                                    return 0f;
                                }
                            }),

                        true when data["base_effect"] is float f =>

                            new List<float>
                            {
                                f
                            },
                        true when data["base_effect"] is int i =>
                            new List<float> { Convert.ToSingle(i) },

                        _ => new List<float>()
                    },
                    stack_effect = data.ContainsKey("stack_effect") switch
                    {
                        true when data["stack_effect"] is List<object> list =>
                            list.ConvertAll(obj =>
                            {
                                try { return Convert.ToSingle(obj); }
                                catch
                                {
                                    Debug.LogWarning($"stack_effect 리스트 내부 변환 실패: {obj} ({obj?.GetType()})");
                                    return 0f;
                                }
                            }),

                        true when data["stack_effect"] is float f =>
                            new List<float> { f },

                        true when data["stack_effect"] is int i =>
                            new List<float> { Convert.ToSingle(i) },

                        _ => new List<float>()
                    }
                };

                items[itemId] = item;     
                
            }
        }
        catch(Exception ex) 
        {
            Debug.LogError($"아이템 로드 오류: {ex}");
        }
        finally
        {
            Debug.Log("아이템 로드 완료");
        }
    }

    //이건 유닛 정보. 사용방식은 위와 동일함
    public static async Task UnitLoadData()
    {
        QuerySnapshot unitDataLoad = await firestore.Collection("units").GetSnapshotAsync();
        try
        {
            foreach (DocumentSnapshot doc in unitDataLoad.Documents)
            {
                Dictionary<string, object> data = doc.ToDictionary();

                int unitId = int.Parse(doc.Id);

                UnitDB unit = new UnitDB
                {
                    name_kr = data["name_kr"].ToString(),
                    name = data["name"].ToString(),
                    type = data["type"].ToString(),
                    tier = Convert.ToInt32(data["tier"]),
                    mana = Convert.ToInt32(data["mana"]),
                    attack_Type = data["attack_Type"].ToString(),
                    attack_Speed = Convert.ToSingle(data["attack_Speed"]),
                    range = Convert.ToInt32(data["range"]),
                    speed = Convert.ToSingle(data["speed"]),
                    spawn_Stage = Convert.ToInt32(data["spawn_Stage"]),

                    synergy = data.ContainsKey("synergy") && data["synergy"] is List<object> rawSynergy
                    ? rawSynergy.ConvertAll(obj => obj.ToString())
                    : new List<string>(),

                    hp = data.ContainsKey("hp") && data["hp"] is List<object> rawHp
                    ? rawHp.ConvertAll(obj => Convert.ToInt32(obj))
                    : new List<int>(),

                    attack = data.ContainsKey("attack") && data["attack"] is List<object> rawAtk
                    ? rawAtk.ConvertAll(obj => Convert.ToInt32(obj))
                    : new List<int>()

                    // 존재 여부에 따라 처리함
                };


                units[unitId] = unit;

                //string log = $"[Unit] {unit.name_kr} ({unit.name})\n" +
                //    $"- Type: {unit.type}, Tier: {unit.tier}, Synergy: [{string.Join(", ", unit.synergy)}]\n" +
                //    $"- HP: [{string.Join("/", unit.hp)}], ATK: [{string.Join("/", unit.attack)}], Mana: {unit.mana}\n" +
                //    $"- AtkType: {unit.attack_Type}, AtkSpeed: {unit.attack_Speed}, Range: {unit.range}, Speed: {unit.speed}\n" +
                //    $"- Spawn Stage: {unit.spawn_Stage}";

                //Debug.Log(log);
            }

        }
        catch (Exception ex)
        {
            Debug.LogWarning($"유닛 로드 오류: {ex}");
        }
        finally
        {
            Debug.Log("유닛 로드 완료");
        }
    }

    //존재하지 않는 ID를 조회해도 게임이 크래시 나지 않도록 예외 처리 포함한 버전

    //+사용예시+
    //var 원하는 변수명(ex.oldDagger) = FirebaseManager.GetItemByID(1101001);
    //oldDagger.name;         // 아이템 이름
    //oldDagger.cooltime;     // 쿨타임

    public static ItemDB GetItemByID(int id)
    {
        if (items.TryGetValue(id, out var item))
            return item;
        else
        {
            Debug.LogWarning($"아이템 ID {id}를 찾을 수 없습니다.");
            return null;
        }

        //그냥 바로바로 items[itemID].value로 사용해도 되고, 확실한게 좋다면 이 방식을 사용하면 됨
    }

    public static UnitDB GetUnitByID(int id)
    {
        if (units.TryGetValue(id, out var unit))
            return unit;
        else
        {
            Debug.LogWarning($"유닛 ID {id}를 찾을 수 없습니다.");
            return null;
        }
    }
}