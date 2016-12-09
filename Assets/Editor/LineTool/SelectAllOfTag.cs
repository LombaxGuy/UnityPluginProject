using UnityEngine;
using System.Collections;
using UnityEditor;

public class SelectAllOfTag : ScriptableWizard {

    public bool testrange = false;
    public int number;
    public GameObject selected;
    public Vector3 objectDimentions;
    public Quaternion objectQuat;

    public Vector3 point1;
    public Vector3 point2;
    public Vector3 point3;
    public float posplus = 0.2f;

    public Vector3[] placements;

    public float incrimentDist;


    [MenuItem("ZoonTools/Select All Of Tag %#g", false, 51)]
    static void SelectAllOfTAgWizard()
    {       
     ScriptableWizard.DisplayWizard<SelectAllOfTag>("SelectAll Of Tag", "Spawn","SetPos");
    }

    void OnWizardCreate()
    {

        Object prefab = selected;
        
        
        for (int i = 0; i < number; i++)
        {
            GameObject copy = Instantiate(prefab, selected.transform.position, selected.transform.rotation) as GameObject;
            copy.transform.rotation = Quaternion.Slerp(copy.transform.rotation, Quaternion.LookRotation(point2), 1);
            posplus = posplus + 0.2f;      
            point3 = Vector3.Lerp(point1, point2, posplus);
            //copy.transform.position = new Vector3((selected.transform.position.x + (selected.GetComponent<Renderer>().bounds.size.x * (i + 1))), copy.transform.position.y, copy.transform.position.z);
            //copy.transform.position = new Vector3((point2.x - point1.x  + (selected.GetComponent<Renderer>().bounds.size.x * (i + 1))), (point2.y - point1.y + (selected.GetComponent<Renderer>().bounds.size.y * (i + 1))), (point2.z - point1.z + (selected.GetComponent<Renderer>().bounds.size.z * (i + 1))));

            //copy.transform.position = new Vector3(point1.x + incrimentDist * (i + 1) ,point1.y + incrimentDist, point1.z + incrimentDist);
            //copy.transform.position = new Vector3(point1.x + (point2.x / (i + 1) ), point1.y, point1.z + (point2.z / (i + 1)));
            copy.transform.position = new Vector3(point3.x, point3.y, point3.z);

            
        }


    }

    void OnWizardOtherButton()
    {
      selected = Selection.activeGameObject;
      objectDimentions = (selected.GetComponent<Renderer>().bounds.size);
      point1 = selected.transform.position;


      incrimentDist = Vector3.Distance(point1, point2);
      incrimentDist = incrimentDist / objectDimentions.x;
      incrimentDist = Mathf.Floor(incrimentDist);


      

    }

    void OnWizardUpdate()
    {

    }


}
