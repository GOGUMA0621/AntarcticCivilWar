using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDistinction : MonoBehaviour //유닛 진영 구별을 위해서 임시로 넣은 클래스
{
    private SpriteRenderer _distinction;
    private string _currentTag;

    // Start is called before the first frame update
    void Start()
    {
        _distinction = GetComponent<SpriteRenderer>();
        _currentTag = this.transform.parent.tag;
        SetColorDistinction();
    }

    // Update is called once per frame
    void Update()
    {
        ChangeColorDistinction();
        //this.transform.localPosition = new Vector3(0, 0, 0); //유닛 위에 띄우기
    }

    void ChangeColorDistinction()
    {
        if (_currentTag != this.transform.parent.tag && this.transform.parent.tag == "Unit") //아군
        {
            _currentTag = this.transform.parent.tag;
            _distinction.color = Color.green;
        }
        if (_currentTag != this.transform.parent.tag && this.transform.parent.tag == "Resistance") //저항군
        {
            _currentTag = this.transform.parent.tag;
            _distinction.color = Color.red;
        }
        if (_currentTag != this.transform.parent.tag && this.transform.parent.tag == "Royal") //왕국군
        {
            _currentTag = this.transform.parent.tag;
            _distinction.color = Color.blue;
        }
    }

    void SetColorDistinction()
    {
        if (_currentTag == "Unit")
        {
            _distinction.color = Color.green;
        }
        if (_currentTag == "Resistance")
        {
            _distinction.color = Color.red;
        }
        if ( _currentTag == "Royal")
        {
            _distinction.color = Color.blue;
        }
        if (_currentTag == "Mercenary")
        {
            _distinction.color = Color.black;
        }
    }
}
