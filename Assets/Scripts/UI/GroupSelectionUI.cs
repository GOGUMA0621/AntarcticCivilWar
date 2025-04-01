using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GroupSelectionUI : MonoBehaviour
{
    [SerializeField] private RectTransform groupViewport;
    [SerializeField] private TextMeshProUGUI groupNameText;
    [SerializeField] private TextMeshProUGUI groupDescriptionText;

    [SerializeField] private GameObject unitGroupPrefab;

    [SerializeField] List<GroupUnitUI> unitGroups = new List<GroupUnitUI>();

    private int currentGroupIndex = 0;

    private void Start()
    {
        currentGroupIndex = 0;
        PreviewGroupUnits(currentGroupIndex);
    }

    public void NextGroup()
    {
        currentGroupIndex++;
        if (currentGroupIndex >= unitGroups.Count)
            currentGroupIndex = 0;
        ClearGroupUnits();
        PreviewGroupUnits(currentGroupIndex);
    }

    public void PreviousGroup()
    {
        currentGroupIndex--;
        if (currentGroupIndex < 0)
            currentGroupIndex = unitGroups.Count - 1;
        ClearGroupUnits();
        PreviewGroupUnits(currentGroupIndex);
    }

    private void PreviewGroupUnits(int index)
    {
        groupNameText.text = unitGroups[index].groupName;
        groupDescriptionText.text = unitGroups[index].description;
        foreach (var unit in unitGroups[index].units.groupUnits)
        {
            GameObject unitGroup = Instantiate(unitGroupPrefab, groupViewport);
            unitGroup.GetComponent<Image>().sprite = unit.pfUnit.GetComponent<Unit>().data.unitIcon;
            unitGroup.GetComponentInChildren<TextMeshProUGUI>().text = unit.count.ToString();
        }
    }

    private void ClearGroupUnits()
    {
        foreach (Transform child in groupViewport)
        {
            Destroy(child.gameObject);
        }
    }

    public void StartSelectedGroup()
    {
        foreach(var unit in unitGroups[currentGroupIndex].units.groupUnits)
        {
            for(int i = 0; i < unit.count; i++)
            {
                PlayerUnitManager.instance.AddUnitPrefabList(unit.pfUnit);
            }
        }
        SceneManager.LoadScene("MainScene");
    }
}
[System.Serializable]
class GroupUnitUI
{
    public string groupName;
    public string description;
    public SpawnUnitsSO units;
}
