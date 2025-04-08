using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using System;


//아이템 정보를 가진 클래스임
public class ItemDB
{
    public int price;
    public int cooltime;
    public float base_effect;
    public float stack_effect;
    public string name_kr;
    public string name;
    public string type;
    public string ability_type;
    public string applied_debuff;
    public string rarity;
    public string effect;
    public string des;
}

// 유닛 정보를 가진 클래스임
public class UnitDB
{
    public int hp;
    public int attack;
    public int mana;
    public int range;
    public int force;
    public int drop_Coin;
    public int spawn_Stage;
    public int attack_Range;
    public float attack_Speed;
    public float speed;
    public string name_kr;
    public string name;
    public string type;
    public string group;
    public string team;
    public string attack_Type;
}

public class FirebaseManager : SingleTonBehaviour<FirebaseManager>
{
    public static QuerySnapshot unitDataLoad;
    public static QuerySnapshot itemDataLoad;

    public Dictionary<int, ItemDB> items = new Dictionary<int, ItemDB>();
    public  Dictionary<int, UnitDB> units = new Dictionary<int, UnitDB>();

    private FirebaseFirestore fb_DB;

    // 파이어베이스 db에 저장되어있는 정보들을 불러오는 과정임. 사용방식은 items[문서ID(아이템 No.)].value;
    public async void ItemLoadData()
    {
        itemDataLoad = await fb_DB.Collection("items").GetSnapshotAsync();

        foreach (DocumentSnapshot doc in itemDataLoad.Documents)
        {
            Dictionary<string, object> data = doc.ToDictionary();

            int itemId = int.Parse(doc.Id); // << 문서ID는 저장될때 문자열로 저장된다네요 그래서 Parse함수 사용

            ItemDB item = new ItemDB 
            {
                // Int.Parse()를 사용할 수 도있지만 파베에서 가져온 데이터는 object형식이라네요
                price = Convert.ToInt32(data["price"]),
                name_kr = data["name_kr"].ToString(),
                name = data["name"].ToString(),
                type = data["type"].ToString(),
                ability_type = data["ability_type"].ToString(),
                rarity = data["rarity"].ToString(),
                effect = data["effect"].ToString(),
                des = data["des"].ToString(),

                // 존재 여부에 따라 처리함
                cooltime = data.ContainsKey("cooltime") ? Convert.ToInt32(data["cooltime"]) : 0,
                applied_debuff = data.ContainsKey("applied_debuff") ? data["applied_debuff"].ToString() : "",
                base_effect = data.ContainsKey("base_effect") ? Convert.ToSingle(data["cooltime"]) : 0,
                stack_effect = data.ContainsKey("stack_effect") ? Convert.ToSingle(data["cooltime"]) : 0
            };

            items[itemId] = item;

        }
    }

    //이건 유닛 정보. 사용방식은 위와 동일함
    public async void UnitLoadData()
    {
        unitDataLoad = await fb_DB.Collection("units").GetSnapshotAsync();

        foreach (DocumentSnapshot doc in unitDataLoad.Documents)
        {
            Dictionary<string, object> data = doc.ToDictionary();

            int unitId = int.Parse(doc.Id);

            UnitDB unit = new UnitDB
            {
                hp = Convert.ToInt32(data["hp"]),
                attack = Convert.ToInt32(data["attack"]),
                range = Convert.ToInt32(data["range"]),
                spawn_Stage = Convert.ToInt32(data["spawn_Stage"]),
                attack_Range = Convert.ToInt32(data["attack_Range"]),
                attack_Speed = Convert.ToSingle(data["attack_Speed"]),
                speed = Convert.ToSingle(data["speed"]),
                name_kr = data["name_kr"].ToString(),
                name = data["name"].ToString(),
                type = data["type"].ToString(),
                group = data["group"].ToString(),
                attack_Type = data["attack_Type"].ToString(),

                // 존재 여부에 따라 처리함
                mana = data.ContainsKey("mana") ? Convert.ToInt32(data["mana"]) : 0,
                force = data.ContainsKey("force") ? Convert.ToInt32(data["force"]) : 0,
                drop_Coin = data.ContainsKey("drop_Coin") ? Convert.ToInt32(data["drop_Coin"]) : 0,
                team = data.ContainsKey("team") ? data["team"].ToString() : ""
            };


            units[unitId] = unit;
        }
    }

    //존재하지 않는 ID를 조회해도 게임이 크래시 나지 않도록 예외 처리 포함한 버전

    //+사용예시+
    //var 원하는 변수명(ex.oldDagger) = FirebaseManager.Instance.GetItemById[1101001];
    //oldDagger.name;         // 아이템 이름
    //oldDagger.cooltime;     // 쿨타임

    public ItemDB GetItemById(int id)
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

    public UnitDB GetUnitById(int id)
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