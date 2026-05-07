using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

using Unity.Mathematics;
using System;

public class Player : MonoBehaviour
{
    [SerializeField]
    float speed = 10f; //移動速度

    [SerializeField]

    float ladderSpeed = 5f;

    [SerializeField]
    float jumpPower = 15f;

    [SerializeField]
    float decelerationRate = 0.99f;

    [SerializeField]
    float decelerationRateOnIce = 0.99f;

    float _DecelerationRate = 0.7f;

    [SerializeField]
    TextMeshProUGUI txt;

    [SerializeField]
    GameObject groundHitObject;

    [SerializeField]
    LayerMask groundLayer;

    [SerializeField]
    AudioClip se_jump;

    [SerializeField]
    AudioClip se_inWater;

    [SerializeField]
    GameObject waterHitObject;

    [SerializeField]
    LayerMask waterLayer;

    [SerializeField]
    Game game;

    [SerializeField]
    GameObject slimeBullet;

    [SerializeField]
    GameObject slimeBulletBat;

    [SerializeField]
    Transform firePosition;

    [SerializeField]
    GameObject PlayerMark;

    [SerializeField]
    GameObject Aim;


    public Rigidbody2D rb;  　//物理挙動

    public RuntimeAnimatorController normalController; // 通常モードのAnimator
    public RuntimeAnimatorController batController; // 蝙蝠モードのAnimators

    Animator anim;　　//アニメーション

    SpriteRenderer sp; //画像反転

    AudioSource snd;

    bool Onground;
    bool prevOnGround;

    int numJump = 0;

    float inWater; //水中にいる時間

    float deadTime = 2f;　//死ぬ時間

    public Vector3 initialPosition;



    public List<GameObject> slimeCount = new List<GameObject>();//本体とクローンを格納
    public float slimeSize = 1;//スライムのサイズと打てる球の数
    [SerializeField] float maxSlimeAmount = 2f;
    public Vector2 defaultSize;//初期サイズ

    //slimebullet
    [SerializeField] GameObject slimeDestoryPar;
    [SerializeField] GameObject slimeClone;//スライムのクローン
    [SerializeField] GameObject slimeCloneBat;
    [SerializeField] int controllSlimeNumber = 0;

    public bool isBatMode = false;

    CircleCollider2D cc2d;

    public Transform[] checkPoints; // 全チェックポイントの位置

    [SerializeField] GameObject slimeTetherPrefab;
    SlimeTether currentTether;

    [SerializeField] float mergeScaleMultiplier = 1.2f;
    [SerializeField] float mergeScaleDuration = 0.18f;
    [SerializeField] float mergeFlashDuration = 0.08f;
    [SerializeField] float mergeHitStopDuration = 0.04f;

    bool isPlayingMergeEffect = false;
    Color defaultColor = Color.white;

    [SerializeField] LayerMask dropPlatformLayer;
    [SerializeField] float dropTime = 0.25f;

    bool isDropping = false;
    Collider2D playerCol;
    Coroutine mergeInvincibleBlinkCoroutine;
    readonly List<Collider2D> ignoredMergeColliders = new List<Collider2D>();

    [SerializeField] float maxSlimeSize = 5f; //上限値。バグ対策

    float defaultMass;



    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sp = GetComponent<SpriteRenderer>();
        snd = gameObject.AddComponent<AudioSource>();
        cc2d = GetComponent<CircleCollider2D>();
        playerCol = GetComponent<Collider2D>();

        defaultColor = sp.color;
        defaultSize = transform.localScale;
        slimeCount.Add(gameObject);

        initialPosition = transform.position;
        _DecelerationRate = decelerationRate;
        defaultMass = rb.mass;

        int id = PlayerPrefs.GetInt("LastCheckpoint", 0);

        if (id < checkPoints.Length)
        {
            if (checkPoints[id] != null)
            {
                transform.position = checkPoints[id].position;
                Debug.Log("リスポーン");
            }
        }
    }



    // Update is called once per frame
    void Update()
    {
        if (!canControl)
        {
            anim.SetBool("Walk", false);
            return;
        }

        CheckGround();
        CheckWater();
        Move();
        jump();
        DropDown();

        if (slimeSize > 0)
        {
            Aiming();
            FiringBullet();
        }

        if (Input.GetKeyDown(KeyCode.C) && slimeCount.Count > 1)
        {
            ChangeClone(controllSlimeNumber);
        }

        UpdataSlimeSize(slimeSize);
        Slimecreate();

        if (isBatMode)
        {
            anim.runtimeAnimatorController = batController;
        }
        else
        {
            anim.runtimeAnimatorController = normalController;
        }
    }


    public void Slimecreate()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {

            /*if(GameObject.FindWithTag("PlayerClone") != null){
                Instantiate(slimeDestoryPar, GameObject.FindWithTag("PlayerClone").transform.position, Quaternion.identity);
                Destroy(GameObject.FindWithTag("PlayerClone"));
            }*/


            GameObject bullet = GameObject.FindWithTag("PlayerBullet");
            if (bullet != null)
            {
                GameObject g = Instantiate(slimeClone, bullet.transform.position, Quaternion.identity);
                slimeCount.Add(g);
                CreateTether(g.transform);
                Destroy(bullet);
            }


            GameObject batBullet = GameObject.FindWithTag("PlayerBulletBat");
            if (batBullet != null)
            {
                GameObject bat = Instantiate(slimeCloneBat, batBullet.transform.position, Quaternion.identity);
                slimeCount.Add(bat);
                Destroy(batBullet);


            }
        }

    }

    [SerializeField] float slimebigg = 0.3f;

    public float AddSlimeWithCap(float amount)
    {
        float addable = Mathf.Min(amount, maxSlimeAmount - slimeSize);
        if (addable <= 0f)
        {
            return 0f;
        }

        slimeSize += addable;
        rb.mass = defaultMass * slimeSize; // ← 修正
        return addable;
    }

    void ConsumeSlime(float amount)
    {
        slimeSize = Mathf.Max(0f, slimeSize - amount);
        rb.mass = defaultMass * slimeSize; // ← 修正
    }

    void UpdataSlimeSize(float size)
    {

        Vector2 newSize = defaultSize * (1f + size * slimebigg);
        transform.localScale = new Vector3(newSize.x, newSize.y, 1f);
    }

    void ChangeClone(int num)
    {
        GameObject next = null;

        if (num == slimeCount.Count - 1)
        {
            next = slimeCount[0];// 最初のスライムを取得
            controllSlimeNumber = 0;// 次に操作するスライムのインデックスを0に設定
            slimeCount[0] = gameObject;// 最初のスライムの位置に現在のスライムを配置
            slimeCount[num] = next; // 現在のスライムを次に操作するスライムの位置に設定
            if (GameObject.FindWithTag("PlayerCloneBat") != null)
            {
                isBatMode = true;

            }

        }
        else
        {
            if (GameObject.FindWithTag("PlayerCloneBat") != null)
            {
                isBatMode = true;

            }

            next = slimeCount[num + 1]; // 次のスライムを取得
            controllSlimeNumber = num + 1;// 次に操作するスライムのインデックスを設定
            slimeCount[num + 1] = gameObject; // 次のスライムの位置に現在のスライムを配置
            slimeCount[num] = next; // 現在のスライムを次の位置に設定

        }

        Vector3 playerPos = transform.position;// 現在のスライムの位置
        Vector3 NextPos = next.transform.position; // 次のスライムの位置

        transform.position = NextPos;  // 現在のスライムの位置を次のスライムの位置に変更
        next.transform.position = playerPos; // 次のスライムの位置を現在のスライムの位置に変更


        float PlayerSize = slimeSize;// 現在のスライムのサイズ
        float NextSize = next.GetComponent<SlimeClone>().size;// 次のスライムのサイズ

        next.GetComponent<SlimeClone>().size = PlayerSize;// 次のスライムのサイズを現在のスライムのサイズに変更
        slimeSize = NextSize; // 現在のスライムのサイズを次のスライムのサイズに変更
    }


    bool ismoving = false;
    void Move()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        Vector2 direction = new Vector2(horizontalInput, 0).normalized;
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
        // キャラクターの下に Raycast を飛ばして地面の角度を取得

        if (Input.GetKey(KeyCode.RightArrow))
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
            anim.SetBool("Walk", true);
            AimFlip(false);
            ismoving = true;


        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            anim.SetBool("Walk", true);
            AimFlip(true);
            ismoving = true;


        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x * _DecelerationRate, rb.velocity.y);
            anim.SetBool("Walk", false);
            anim.SetBool("Collapse", false);
            ismoving = false;

        }
    }

    void CreateTether(Transform cloneTransform)
    {
        if (currentTether != null)
        {
            Destroy(currentTether.gameObject);
        }

        GameObject tetherObj = Instantiate(slimeTetherPrefab);
        currentTether = tetherObj.GetComponent<SlimeTether>();
        currentTether.Setup(transform, cloneTransform);
    }


    void jump()
    {
        //zキーが押された瞬間
        if (Input.GetKeyDown(KeyCode.Z))
        {//getkeyは押されている間　getkeydownは押した瞬間
            if (Onground)
            {
                float jumpRestraint = rb.velocity.y;
                float newVerticalVelocity = Mathf.Min(jumpRestraint + jumpPower, jumpPower);
                rb.velocity = new Vector2(rb.velocity.x, newVerticalVelocity);
                numJump++;　//ジャンプの回数
                snd.PlayOneShot(se_jump);
                anim.SetBool("Jump", true);
            }
            else if (numJump == 1 && isBatMode)
            {
                float jumpRestraint = rb.velocity.y;
                float newVerticalVelocity = Mathf.Min(jumpRestraint + jumpPower, jumpPower);
                rb.velocity = new Vector2(rb.velocity.x, newVerticalVelocity);
                numJump++;
                snd.PlayOneShot(se_jump);
                anim.ResetTrigger("Jump"); // 既存のジャンプトリガーをリセット
                anim.SetTrigger("JumpTwo"); // 二段ジャンプのアニメーションをセット

            }
            else if (numJump == 2 && isBatMode)
            {
                float jumpRestraint = rb.velocity.y;
                float newVerticalVelocity = Mathf.Min(jumpRestraint + jumpPower, jumpPower);
                rb.velocity = new Vector2(rb.velocity.x, newVerticalVelocity);
                numJump++;
                snd.PlayOneShot(se_jump);
                anim.ResetTrigger("Jump"); // 既存のジャンプトリガーをリセット
                anim.SetTrigger("JumpTwo"); // 二段ジャンプのアニメーションをセット

            }

        }

        if (!prevOnGround && Onground)
        {
            numJump = 0;
            anim.SetBool("Jump", false);


        }
    }



    void CheckGround()
    {
        prevOnGround = Onground;
        Onground = Physics2D.OverlapCircle(groundHitObject.transform.position, 0.3f, groundLayer);

        //txt.text = Onground.ToString();

        if (Onground)
        {
            //txt.text = "Ground";
            GameObject col = Physics2D.OverlapCircle(groundHitObject.transform.position, 0.6f, groundLayer).gameObject;
            if (col.tag == "Ice")
            {
                _DecelerationRate = decelerationRateOnIce;
            }
            else
            {
                _DecelerationRate = decelerationRate;
            }

        }
        //        else { txt.text = "Air" + numJump; }
        //txt.textに続けて文字を追加
        //      txt.text += "\nWater:" + inWater.ToString("f1");

    }


    void CheckWater()
    {
        //水面と水面判定オブジェクトの設定
        if (Physics2D.OverlapCircle(waterHitObject.transform.position, 0.01f, waterLayer))
        {
            if (inWater == 0) { snd.PlayOneShot(se_inWater); }
            inWater += Time.deltaTime;
            if (inWater >= deadTime)
            {
                //ゲームオーバー処理
                rb.bodyType = RigidbodyType2D.Kinematic;

                rb.velocity = new Vector2(0, 0);
                anim.speed = 0;
                game.StartGameover();
            }
        }
        else
        {
            inWater = 0;
        }
    }

    public void Reset()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        transform.position = initialPosition;


        rb.velocity = new Vector2(0, 0);
        numJump = 0;
        //anim.speed = 0;
    }

    //OnCollisionStay関数

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            rb.gravityScale = 0;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            if (Input.GetKey(KeyCode.UpArrow))
            {
                rb.velocity = new Vector2(rb.velocity.x, ladderSpeed);
                anim.SetBool("Walk", true);
                AimFlip(false);
                ismoving = true;

            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                rb.velocity = new Vector2(rb.velocity.x, -ladderSpeed);
                anim.SetBool("Walk", true);
                AimFlip(false);
                ismoving = true;

            }
        }
    }

    public bool canControl = true;

    public void SetControl(bool value)
    {
        canControl = value;

        if (!canControl)
        {
            rb.velocity = Vector2.zero;
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            rb.gravityScale = 4; // 重力を元に戻す
        }
    }




    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Exit")
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = new Vector2(0, 0);
            //anim.speed = 0;
            game.StartGameclear();

        }



        if (collision.gameObject.tag == "PlayerClone")
        {
            if (isMergeInvincible) return;

            SlimeClone clone = collision.gameObject.GetComponent<SlimeClone>();
            if (clone == null || !clone.TryBeginMerge()) return;

            slimeCount.Remove(collision.gameObject);
            AddSlimeWithCap(clone.size + 1);
            CloneReset();
            Destroy(collision.gameObject);

            if (currentTether != null && currentTether.IsConnectedTo(collision.gameObject))
            {
                Destroy(currentTether.gameObject);
                currentTether = null;
            }
        }

        if (collision.gameObject.tag == "PlayerCloneBat")
        {
            if (isMergeInvincible) return;

            SlimeClone clone = collision.gameObject.GetComponent<SlimeClone>();
            if (clone == null || !clone.TryBeginMerge()) return;

            isBatMode = true;
            slimeCount.Remove(collision.gameObject);
            AddSlimeWithCap(clone.size + 1);
            CloneReset();
            Destroy(collision.gameObject);
        }


        if (collision.gameObject.tag == "PlayerBullet")
        {
            if (collision.gameObject.GetComponent<SlimeBullet>().droping)
            {
                Destroy(collision.gameObject);
                AddSlimeWithCap(1f);
            }
        }

        if (collision.gameObject.tag == "PlayerBulletBat")
        {
            if (collision.gameObject.GetComponent<SlimeBulletBat>())
            {

                Destroy(collision.gameObject);
                AddSlimeWithCap(1f);

                numJump = 0; // 取得時にリセット
                isBatMode = true;

            }
        }

        if (collision.gameObject.tag == "Bat")
        {
            StartCoroutine(FlashWhite());
            StartCoroutine(InvincibilityTime());


        }

        if (collision.gameObject.tag == "Wall" && ismoving)
        { anim.SetBool("Collapse", true); }


    }

    public IEnumerator FlashWhite()
    {

        for (int i = 0; i < 4; i++)
        {
            sp.enabled = false;

            yield return new WaitForSeconds(0.1f);
            sp.enabled = true;

            yield return new WaitForSeconds(0.1f);

        }


    }

    public IEnumerator InvincibilityTime()
    {
        // 自分のColliderを取得
        Collider2D playerCol = GetComponent<Collider2D>();

        // すべてのBatとの衝突を無視
        GameObject[] bats = GameObject.FindGameObjectsWithTag("Bat");
        foreach (GameObject bat in bats)
        {
            Collider2D batCol = bat.GetComponent<Collider2D>();
            if (batCol != null)
            {
                Physics2D.IgnoreCollision(playerCol, batCol, true);
            }
        }

        yield return new WaitForSeconds(1f);

        // 衝突を元に戻す
        foreach (GameObject bat in bats)
        {
            Collider2D batCol = bat.GetComponent<Collider2D>();
            if (batCol != null)
            {
                Physics2D.IgnoreCollision(playerCol, batCol, false);
            }
        }
    }

    void CloneReset()
    {
        for (int i = 0; i < slimeCount.Count; i++)
        {
            if (gameObject == slimeCount[i])
            {
                controllSlimeNumber = i;
            }
        }
    }

    public void FiringBullet()
    {

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (isBatMode == false)
            {
                GameObject bullet;

                bullet = Instantiate(slimeBullet, transform.position, Quaternion.identity);

                Vector3 distance = (shootPoint.position - transform.position).normalized;
                bullet.GetComponent<Rigidbody2D>().AddForce(distance * shootPower, ForceMode2D.Impulse);
                anim.SetTrigger("Throw");

                slimeSize--;
                NormalizeSlimeState();
                aiming.SetActive(false);
                AimingRotationTimer = 0;
            }

            if (isBatMode)
            {
                GameObject bulletBat;

                bulletBat = Instantiate(slimeBulletBat, transform.position, Quaternion.identity);

                Vector3 a = (shootPoint.position - transform.position).normalized;
                bulletBat.GetComponent<Rigidbody2D>().AddForce(a * shootPower, ForceMode2D.Impulse);
                anim.SetTrigger("Throw");

                slimeSize--;
                NormalizeSlimeState();
                aiming.SetActive(false);
                AimingRotationTimer = 0;
                isBatMode = false;

            }




        }
    }

    [SerializeField] GameObject yajirushi;
    [SerializeField] Transform shootPoint;
    [SerializeField] float shootPower = 10;

    [SerializeField] float AimingRotationTime = 1;
    float AimingRotationTimer = 0;

    bool AimingFlagg = true;

    [SerializeField] GameObject aiming;

    [SerializeField] GameObject target;

    public void Aiming()
    {

        target.transform.position = shootPoint.position;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            aiming.SetActive(true);
            AimingRotationTimer = 0;
            AimingFlagg = true;



        }


        if (Input.GetKey(KeyCode.Space))
        {

            AimingRotationTimer += Time.deltaTime;
            Vector3 a = new Vector3(0, yajirushi.transform.localRotation.eulerAngles.y, 0);
            Vector3 b = new Vector3(0, yajirushi.transform.localRotation.eulerAngles.y, 90);





            if (AimingFlagg == true)
            {

                yajirushi.transform.localRotation = Quaternion.Euler(Vector3.Lerp(a, b, AimingRotationTimer / AimingRotationTime));

                if (AimingRotationTimer >= AimingRotationTime)
                {
                    AimingFlagg = false;
                    AimingRotationTimer = 0;
                }
            }
            else
            {
                yajirushi.transform.localRotation = Quaternion.Euler(Vector3.Lerp(b, a, AimingRotationTimer / AimingRotationTime));

                if (AimingRotationTimer >= AimingRotationTime)
                {
                    AimingFlagg = true;
                    AimingRotationTimer = 0;
                }
            }

        }

    }

    void AimFlip(bool boo)
    {
        if (sp.flipX == true && boo == false)
        {
            sp.flipX = false;

            Quaternion rot = yajirushi.transform.localRotation;
            yajirushi.transform.localRotation = Quaternion.Euler(new Vector3(rot.x, 0, rot.z));
        }
        else if (sp.flipX == false && boo == true)
        {
            sp.flipX = true;
            Quaternion rot = yajirushi.transform.localRotation;
            yajirushi.transform.localRotation = Quaternion.Euler(new Vector3(rot.x, 180, rot.z));
        }
    }

    public void PlayMergeEffect()
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(PlayMergeEffectCoroutine());
    }

    IEnumerator PlayMergeEffectCoroutine()
    {
        if (isPlayingMergeEffect) yield break;
        isPlayingMergeEffect = true;

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(mergeHitStopDuration);
        Time.timeScale = 1f;

        Vector3 baseScale = defaultSize;
        Vector3 bigScale = defaultSize * mergeScaleMultiplier;

        sp.color = Color.white;

        float timer = 0f;
        while (timer < mergeScaleDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / mergeScaleDuration;

            if (t < 0.5f)
            {
                transform.localScale = Vector3.Lerp(baseScale, bigScale, t / 0.5f);
            }
            else
            {
                transform.localScale = Vector3.Lerp(bigScale, baseScale, (t - 0.5f) / 0.5f);
            }

            if (timer >= mergeFlashDuration)
            {
                sp.color = defaultColor;
            }

            yield return null;
        }

        transform.localScale = baseScale;
        sp.color = defaultColor;
        isPlayingMergeEffect = false;
    }

    void DropDown()
    {
        if (!Input.GetKeyDown(KeyCode.DownArrow) && !Input.GetKeyDown(KeyCode.S)) return;

        Debug.Log("Down pressed");
        Debug.Log("Onground: " + Onground);

        if (!Onground || isDropping) return;

        Collider2D platformCol = Physics2D.OverlapCircle(
            groundHitObject.transform.position,
            0.3f,
            dropPlatformLayer
        );

        Debug.Log("Platform: " + platformCol);

        if (platformCol != null)
        {
            StartCoroutine(DropPlatformCoroutine(platformCol));
        }
    }

    IEnumerator DropPlatformCoroutine(Collider2D platformCol)
    {
        isDropping = true;

        platformCol.enabled = false;

        rb.velocity = new Vector2(rb.velocity.x, -1f);

        yield return new WaitForSeconds(0.3f);

        platformCol.enabled = true;

        isDropping = false;
    }

    //プレイヤー再配置
    bool isRespawning = false;
    [SerializeField] float respawnDelay = 0.2f;
    [SerializeField] GameObject triangle;
    bool isMergeInvincible = false;
    [SerializeField] float mergeInvincibleTime = 0.5f;

    List<Collider2D> GetCloneColliders()
    {
        List<Collider2D> cloneColliders = new List<Collider2D>();

        for (int i = 0; i < slimeCount.Count; i++)
        {
            GameObject slime = slimeCount[i];
            if (slime == null || slime == gameObject) continue;

            Collider2D cloneCollider = slime.GetComponent<Collider2D>();
            if (cloneCollider != null)
            {
                cloneColliders.Add(cloneCollider);
            }
        }

        return cloneColliders;
    }

    void SetCloneCollisionIgnored(bool ignore)
    {
        if (ignore)
        {
            ignoredMergeColliders.Clear();

            List<Collider2D> cloneColliders = GetCloneColliders();
            for (int i = 0; i < cloneColliders.Count; i++)
            {
                Collider2D cloneCollider = cloneColliders[i];
                Physics2D.IgnoreCollision(playerCol, cloneCollider, true);
                ignoredMergeColliders.Add(cloneCollider);
            }

            return;
        }

        for (int i = ignoredMergeColliders.Count - 1; i >= 0; i--)
        {
            Collider2D cloneCollider = ignoredMergeColliders[i];
            if (cloneCollider != null)
            {
                Physics2D.IgnoreCollision(playerCol, cloneCollider, false);
            }
        }

        ignoredMergeColliders.Clear();
    }

    IEnumerator MergeInvincibleBlink()
    {
        while (isMergeInvincible)
        {
            sp.enabled = false;
            yield return new WaitForSeconds(0.1f);
            sp.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }

        sp.enabled = true;
    }

    public void RespawnAt(Vector3 position)
    {
        if (!isRespawning)
        {
            StartCoroutine(RespawnCoroutine(position));
        }
    }

    IEnumerator RespawnCoroutine(Vector3 position)
    {
        isRespawning = true;
        isMergeInvincible = true;
        SetControl(false);

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        playerCol.enabled = false;

        Instantiate(slimeDestoryPar, transform.position, Quaternion.identity);

        Color c = sp.color;
        c.a = 0f;
        sp.color = c;
        triangle.SetActive(false);

        yield return new WaitForSeconds(respawnDelay);

        transform.position = position;

        c.a = 1f;
        sp.color = c;
        triangle.SetActive(true);

        playerCol.enabled = true;
        SetControl(true);
        SetCloneCollisionIgnored(true);

        if (mergeInvincibleBlinkCoroutine != null)
        {
            StopCoroutine(mergeInvincibleBlinkCoroutine);
        }
        mergeInvincibleBlinkCoroutine = StartCoroutine(MergeInvincibleBlink());

        yield return new WaitForSeconds(mergeInvincibleTime);

        isMergeInvincible = false;
        SetCloneCollisionIgnored(false);
        if (mergeInvincibleBlinkCoroutine != null)
        {
            StopCoroutine(mergeInvincibleBlinkCoroutine);
            mergeInvincibleBlinkCoroutine = null;
        }
        sp.enabled = true;
        isRespawning = false;
    }

    public void RespawnAllSlimes(Vector3 playerPosition, Vector3 clonePosition)
    {
        StartCoroutine(RespawnAllSlimesCoroutine(playerPosition, clonePosition));
    }

    IEnumerator RespawnAllSlimesCoroutine(Vector3 playerPosition, Vector3 clonePosition)
    {
        if (isRespawning) yield break;

        isRespawning = true;
        isMergeInvincible = true;
        SetControl(false);

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        playerCol.enabled = false;

        Color c = sp.color;
        c.a = 0f;
        sp.color = c;
        triangle.SetActive(false);

        for (int i = 0; i < slimeCount.Count; i++)
        {
            if (slimeCount[i] == null || slimeCount[i] == gameObject) continue;

            Rigidbody2D cloneRb = slimeCount[i].GetComponent<Rigidbody2D>();
            Collider2D cloneCol = slimeCount[i].GetComponent<Collider2D>();
            SpriteRenderer cloneSp = slimeCount[i].GetComponent<SpriteRenderer>();

            if (cloneRb != null)
            {
                cloneRb.velocity = Vector2.zero;
                cloneRb.angularVelocity = 0f;
            }

            if (cloneCol != null) cloneCol.enabled = false;

            if (cloneSp != null)
            {
                Color cloneColor = cloneSp.color;
                cloneColor.a = 0f;
                cloneSp.color = cloneColor;
            }
        }

        yield return new WaitForSeconds(respawnDelay);

        transform.position = playerPosition;

        c.a = 1f;
        sp.color = c;
        triangle.SetActive(true);
        playerCol.enabled = true;

        for (int i = 0; i < slimeCount.Count; i++)
        {
            if (slimeCount[i] == null || slimeCount[i] == gameObject) continue;

            slimeCount[i].transform.position = clonePosition;

            Collider2D cloneCol = slimeCount[i].GetComponent<Collider2D>();
            SpriteRenderer cloneSp = slimeCount[i].GetComponent<SpriteRenderer>();

            if (cloneCol != null) cloneCol.enabled = true;

            if (cloneSp != null)
            {
                Color cloneColor = cloneSp.color;
                cloneColor.a = 1f;
                cloneSp.color = cloneColor;
            }
        }

        SetControl(true);
        SetCloneCollisionIgnored(true);

        yield return new WaitForSeconds(mergeInvincibleTime);

        isMergeInvincible = false;
        SetCloneCollisionIgnored(false);
        sp.enabled = true;
        isRespawning = false;
    }

    public void NormalizeSlimeState()
    {
        slimeSize = Mathf.Clamp(slimeSize, 0f, maxSlimeSize);
        rb.mass = defaultMass * slimeSize;
    }






}
