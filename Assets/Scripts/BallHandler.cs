using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallHandler : MonoBehaviour
{
    //Sadece private yazarsak Unity'de degeri goremeyiz. Ama Serializefield ile Unity'de arayuzde gorebiliriz.
    //sadce public yazilsaydi onda da Unity'de goruruz.
    //Ama guvenlik acisindan private ile Serializefield kullanarak degeri gormek daha guvenli.
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Rigidbody2D pivot;
    [SerializeField] private float DetachDelay;
    [SerializeField] private float respawnDelay;

    private Rigidbody2D currentBallRigidbody;
    private SpringJoint2D currentBallSpringJoint;

    private Camera mainCamera;
    private bool isDragging;
    
    void Start()
    {
        mainCamera = Camera.main;
        SpawnNewBall();
    }

    
    void Update()
    {
        if(currentBallRigidbody == null)   //Yani top yok ise geri doner ve oyun durur
        {
            return;
        }

        if (!Touchscreen.current.primaryTouch.press.isPressed)   //Eger ekranda dokunma olunmazsa
        {
            //Topa dokunduk ve surukledik, elimizi cektigimiz anda suruklenme devam ediyorsa launch ball devreye girer.
            if (isDragging) 
            {
                LaunchBall();
            }

            isDragging = false;
            return;
        }

        //Eger ekranda dokunma varsa asagidakiler yapilir
        isDragging = true;
        currentBallRigidbody.isKinematic = true;   //topun dynamic'ten kinematic'e gecis yapmasini sagladi

        Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(touchPosition);  //touchposition Vector3 yaparak 3D'ye gecirdik. Buna world position diyoruz.

        currentBallRigidbody.position = worldPosition;        
    }

    //SpawnNewBall metodu ile bircok top uretilmesini sagliyoruz.
    //Once bir top uretilir, daha sonra bloklara top firlatilir. Daha sonra ise tekrardan yerine baska bir top uretilit.
    //Instantiate dinamik olarak obje uretilmesini saglar.
    //Ornegin dusman uretmek, mermi uretmek, top uretmek diyebiliriz
    //Bu uretimleri onceden olusturdugumuz prebas ile yapariz.
    //Quaternion.identity dondurulmemis anlamına gelir
    private void SpawnNewBall()
    {
        GameObject ballInstance = Instantiate(ballPrefab, pivot.position, Quaternion.identity);
        currentBallRigidbody = ballInstance.GetComponent<Rigidbody2D>();
        currentBallSpringJoint = ballInstance.GetComponent<SpringJoint2D>();

        currentBallSpringJoint.connectedBody = pivot; //pivota baglanarak topun firlatilmasini saglar
    }


    //LaunchBall topu serbest bırakmayı ve top fırlatilmadan once on asamanin saglanmasını saglar
    //isKinematic = false topun fiziksel olarak (yer cekimi, carpisma) gibi etkilerle karsilasmasini saglar.
    private void LaunchBall()
    {
        currentBallRigidbody.isKinematic = false; 
        currentBallRigidbody = null;

        /*Invoke'un 2 parametresi var. 1.'si metod adi, 2.'si ise süre.
        Invoke("MethodDelay", methodDuration)
        Invoke("DetachBall", 0.5f); yazabiliriz.
        MethodName'de nameOf kullanmamizin sebebi metodun yanlıs yazarsak uyari vermesi icin.
        Cunku nameOf yazmazsak metod adi yanlis yazarsak uyari veriyor.*/
        Invoke(nameof(DetachBall), DetachDelay);
    }

    
    //detachBall topu ayirmak demek
    //Topu ayirdigimizde spring'i engelliyoruz ve null yapiyoruz.
    private void DetachBall()
    {
        currentBallSpringJoint.enabled = false;
        currentBallSpringJoint = null;

        Invoke(nameof(SpawnNewBall), respawnDelay);
    }

}
