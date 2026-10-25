using UnityEngine;

public class TutorialMethod : MonoBehaviour
{
    public TutorialList list;

    public BoxCollider range;

    public int i = 0;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            foreach (GameObject item in list.tutorials)
            {
                if (i == list.colliderCount && list.tutorials[0])
                {
                    list.tutorials[i].SetActive(true);
                }

                else if (i == list.colliderCount && list.tutorials[0]!)
                {
                    if (list.tutorials[i - 1].activeSelf!)
                    {
                        list.tutorials[i].SetActive(true);
                    }

                    else
                    {
                        list.tutorials[i].SetActive(false);
                    }
                }

                else
                {
                    i++;
                }
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            foreach (GameObject item in list.tutorials)
            {
                if (i == list.colliderCount && list.tutorials[0])
                {
                    list.tutorials[i].SetActive(false);
                    list.colliderCount += 1;
                    range.enabled = false;
                }

                else if (i == list.colliderCount && list.tutorials[0]!)
                {

                    list.tutorials[i].SetActive(false);
                    //list.colliderCount++;
                    range.enabled = false;
                }
            }
        }
   
    }
}



