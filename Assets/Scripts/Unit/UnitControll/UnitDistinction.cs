using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDistinction : MonoBehaviour
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
    }

    void ChangeColorDistinction()
    {
        if (_currentTag != this.transform.parent.tag && this.transform.parent.tag == "Unit")
        {
            _currentTag = this.transform.parent.tag;
            _distinction.color = Color.green;
        }
        if (_currentTag != this.transform.parent.tag && this.transform.parent.tag == "Enemy")
        {
            _currentTag = this.transform.parent.tag;
            _distinction.color = Color.red;
        }
    }

    void SetColorDistinction()
    {
        if (_currentTag == "Unit")
        {
            _distinction.color = Color.green;
        }
        if (_currentTag == "Enemy")
        {
            _distinction.color = Color.red;
        }
    }
}
