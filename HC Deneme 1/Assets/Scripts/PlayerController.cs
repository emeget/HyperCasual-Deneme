using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Current;

    public float limitX;

    public float runningSpeed;
    public float xSpeed;
    private float _currentRunningSpeed;

    public GameObject ridingCylinderPrefab;
    public List<RidingCylinder> cylinders;

    private bool _spawningBridge;
    public GameObject bridgePiecePrefab;
    private BridgeSpawner _bridgeSpawner;
    private float _creatingBridgeTimer;

    private bool _finished;

    private float _scoreTimer = 0;

    public Animator animator;

    void Start()
    {
        Current = this;

    }

    // Update is called once per frame
    void Update()
    {
        if (LevelController.Current == null || !LevelController.Current.gameActive)
        {
            return;
        }
        float newX = 0;
        float touchXDelta = 0;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            touchXDelta = Input.GetTouch(0).deltaPosition.x / Screen.width;
        }
        else if (Input.GetMouseButton(0))
        {
            touchXDelta = Input.GetAxis("Mouse X");
        }
        newX = transform.position.x + xSpeed * touchXDelta * Time.deltaTime;
        newX = Mathf.Clamp(newX, -limitX, limitX);

        Vector3 newPosition = new Vector3(newX, transform.position.y, transform.position.z + _currentRunningSpeed * Time.deltaTime);
        transform.position = newPosition;


        if (_spawningBridge)
        {
            _creatingBridgeTimer -= Time.deltaTime;
            if (_creatingBridgeTimer < 0)
            {
                _creatingBridgeTimer = 0.01f;
                IncrementCylinderVolume(-0.01f);
                GameObject createdBridgePiece = Instantiate(bridgePiecePrefab);
                Vector3 direction = _bridgeSpawner.endReference.transform.position - _bridgeSpawner.startReference.transform.position;
                float distance = direction.magnitude;
                direction = direction.normalized;
                createdBridgePiece.transform.forward = direction;
                float characterDistance = transform.position.z - _bridgeSpawner.startReference.transform.position.z;
                characterDistance = Mathf.Clamp(characterDistance, 0, distance);
                Vector3 newPiecePosition = _bridgeSpawner.startReference.transform.position + direction * characterDistance;
                newPiecePosition.x = transform.position.x;
                createdBridgePiece.transform.position = newPiecePosition;


                if (_finished)
                {
                    _scoreTimer -= Time.deltaTime;
                    if (_scoreTimer < 0)
                    {
                        _scoreTimer = 0.3f;
                        LevelController.Current.ChangeScore(1);
                    }
                }
            }
        }
    }



        public void ChangeSpeed(float value)
        {
            _currentRunningSpeed = value;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "AddCylinder")
            {
                IncrementCylinderVolume(0.1f);
                Destroy(other.gameObject);
            }
            else if (other.tag == "SpawnBridge")
            {
                StartSpawningBridge(other.transform.parent.GetComponent<BridgeSpawner>());
            }
            else if (other.tag == "StopSpawnBridge")
            {
                StopSpawningBridge();
                if (_finished)
                {
                    LevelController.Current.FinishGame();
                }
            }
            else if (other.tag == "Finish")
            {
                _finished = true;
                StartSpawningBridge(other.transform.parent.GetComponent<BridgeSpawner>());
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (other.tag == "Trap")
            {
                IncrementCylinderVolume(-Time.fixedDeltaTime);
            }
        }

        public void IncrementCylinderVolume(float value)
        {
            if (cylinders.Count == 0)
            {
                if (value > 0)
                {
                    CreateCylinder(value);
                }
                else
                {
                    if (_finished)
                    {
                        LevelController.Current.FinishGame();
                    }
                    else
                    {
                    Die();
                    }
                }
            }

            else
            {
                cylinders[cylinders.Count - 1].IncrementCylinderVolume(value);
            }
        }
    public void Die()
    {
        animator.SetBool("dead", true);
        gameObject.layer = 6;
        Camera.main.transform.SetParent(null);
        LevelController.Current.GameOver();

    }

    public void CreateCylinder(float value)
        {
            RidingCylinder createdCylinder = Instantiate(ridingCylinderPrefab, transform).GetComponent<RidingCylinder>();

            cylinders.Add(createdCylinder);
            createdCylinder.IncrementCylinderVolume(value);
        }
        public void DestroyCylinder(RidingCylinder cylinder)
        {
            cylinders.Remove(cylinder);
            Destroy(cylinder.gameObject);
        }
    public void StartSpawningBridge(BridgeSpawner spawner)
    {
        _bridgeSpawner = spawner;
        _spawningBridge = true;
    }
    public void StopSpawningBridge()
    {
        _spawningBridge = false;
    }
    }


