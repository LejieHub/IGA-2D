using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    private Animator anim => GetComponent<Animator>();
    //public string currentSceneName;
    public string nextSceneName;

    // Start is called before the first frame update
    void Start()
    {
        //anim = GetComponent<Animator>();
        //anim.SetTrigger("Switch");

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.anyKeyDown)
        //{
        //    LoadScene();
        //}
    }

    public void ReloadThisScene()
    {
        StartCoroutine(Reload());
    }

    public void LoadNextScene()
    {
        StartCoroutine(LoadNext());
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("�˳���Ϸ");
    }
    IEnumerator LoadNext()//Э��
    {
        anim.SetTrigger("Switch");
        yield return new WaitForSeconds(1.1f);
        SceneManager.LoadScene(nextSceneName);//���ز�����ת��������Level��
    }
    IEnumerator Reload()
    {
        anim.SetTrigger("Switch");
        yield return new WaitForSeconds(1.1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);//���ز�����ת����������Level��
    }
}
