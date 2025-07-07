#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UnitUploaderEditor : EditorWindow
{
    private string unitId = "2109999";
    private string name_kr = "테스트 유닛";
    private string name = "TestUnit";
    private string type = "normal";
    private int tier = 1;
    private int mana = 0;
    private string attack_Type = "melee";
    private float attack_Speed = 0.7f;
    private int range = 1;
    private float speed = 3.0f;
    private int spawn_Stage = 1;

    private string synergyRaw = "Royal,Warrior";
    private string hpRaw = "550,950,1800";
    private string atkRaw = "40,65,110";

    [MenuItem("Tools/파이어베이스 유닛 업로더")]
    public static void ShowWindow()
    {
        GetWindow<UnitUploaderEditor>("Unit Uploader");
    }

    private void OnGUI()
    {
        GUILayout.Label("Firestore 유닛 데이터 작성", EditorStyles.boldLabel);

        unitId = EditorGUILayout.TextField("Unit ID", unitId);
        name_kr = EditorGUILayout.TextField("이름 (한글)", name_kr);
        name = EditorGUILayout.TextField("이름 (영문)", name);
        type = EditorGUILayout.TextField("타입", type);
        tier = EditorGUILayout.IntField("티어", tier);
        synergyRaw = EditorGUILayout.TextField("시너지 (쉼표로 구분)", synergyRaw);
        hpRaw = EditorGUILayout.TextField("HP (쉼표로 구분)", hpRaw);
        atkRaw = EditorGUILayout.TextField("ATK (쉼표로 구분)", atkRaw);
        mana = EditorGUILayout.IntField("마나", mana);
        attack_Type = EditorGUILayout.TextField("공격 타입", attack_Type);
        attack_Speed = EditorGUILayout.FloatField("공격 속도", attack_Speed);
        range = EditorGUILayout.IntField("사거리", range);
        speed = EditorGUILayout.FloatField("이동 속도", speed);
        spawn_Stage = EditorGUILayout.IntField("스폰 스테이지", spawn_Stage);


        if (GUILayout.Button("파이어스토어에 업로드"))
        {
            UploadUnit();
        }
    }

    private async void UploadUnit()
    {
        List<string> synergy = new List<string>(synergyRaw.Split(','));
        List<int> hp = ParseIntList(hpRaw);
        List<int> attack = ParseIntList(atkRaw);

        Dictionary<string, object> unitData = new()
        {
            { "name_kr", name_kr },
            { "name", name },
            { "type", type },
            { "tier", tier },
            { "mana", mana },
            { "attack_Type", attack_Type },
            { "attack_Speed", attack_Speed },
            { "range", range },
            { "speed", speed },
            { "spawn_Stage", spawn_Stage },
            { "synergy", synergy },
            { "hp", hp },
            { "attack", attack }
        };

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        await db.Collection("units").Document(unitId).SetAsync(unitData);
        Debug.Log($"Firestore에 유닛 {name_kr} 업로드 완료 (ID: {unitId})");
    }

    private List<int> ParseIntList(string csv)
    {
        List<int> result = new();
        string[] parts = csv.Split(',');
        foreach (var part in parts)
        {
            if (int.TryParse(part.Trim(), out int val))
                result.Add(val);
        }
        return result;
    }
}
#endif