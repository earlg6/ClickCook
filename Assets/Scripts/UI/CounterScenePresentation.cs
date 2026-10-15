using ClickCook.Runtime;
using UnityEngine;

namespace ClickCook.UI
{
    /// <summary>Scene-only presentation of the existing order session. No order, reward or damage logic.</summary>
    public sealed class CounterScenePresentation : MonoBehaviour
    {
        [SerializeField] private ClickCookPrototypeController controller;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private CameraCursorFollow cursorFollow;
        [SerializeField] private GameObject workDayScreen, cookingScreen, resultScreen;
        [SerializeField] private RectTransform boardCanvas;
        [SerializeField] private Renderer boardPaperLeft, boardPaperRight, plate;
        [SerializeField] private GameObject dishRoot, sandwich, mugCake, genericDish;
        [SerializeField] private Collider dishCollider;
        [SerializeField] private UnityEngine.UI.Button[] sheets;
        [SerializeField] private float boardYawBias = 12f;
        private Quaternion returnRotation;
        private float returnFov;
        private bool followWasEnabled, leftWasEnabled, rightWasEnabled, focused, initialized, wasBoard, wasCooking;

        private void Start()
        {
            if (controller == null || sceneCamera == null || boardCanvas == null || plate == null ||
                boardPaperLeft == null || boardPaperRight == null || dishRoot == null || dishCollider == null ||
                sandwich == null || mugCake == null || genericDish == null || workDayScreen == null ||
                cookingScreen == null || resultScreen == null || sheets == null || sheets.Length != 6)
            { Debug.LogError("Counter presentation is missing scene bindings.",this); enabled=false; return; }
            foreach (var sheet in sheets)
                if (!HasSheetBindings(sheet))
                { Debug.LogError("Counter presentation has an incomplete order sheet.", this); enabled = false; return; }
            leftWasEnabled=boardPaperLeft.enabled;rightWasEnabled=boardPaperRight.enabled;initialized=true;
        }

        private void LateUpdate()
        {
            if(!initialized)return;
            bool cooking=cookingScreen.activeInHierarchy;
            bool result=resultScreen.activeInHierarchy;
            bool board=workDayScreen.activeInHierarchy;
            bool showSheets=board;
            boardCanvas.gameObject.SetActive(showSheets);
            if(board&&!wasBoard)
            {
                foreach(var sheet in sheets)
                    if(sheet.gameObject.activeInHierarchy&&sheet.interactable)
                    { var events=UnityEngine.EventSystems.EventSystem.current;if(events!=null)events.SetSelectedGameObject(sheet.gameObject);break; }
            }
            wasBoard=board;
            boardPaperLeft.enabled=showSheets?false:leftWasEnabled;
            boardPaperRight.enabled=showSheets?false:rightWasEnabled;
            // Anchors follow the actual environment, not a viewport coordinate.
            boardCanvas.rotation=boardPaperLeft.transform.rotation*Quaternion.Euler(0,180,0);
            boardCanvas.position=(boardPaperLeft.bounds.center+boardPaperRight.bounds.center)*.5f + Vector3.up*.04f-boardCanvas.forward*.055f;
            int count=controller.OrderSession.BoardCapacity;
            int columns=Mathf.Min(3,count);
            for(int i=0;i<count;i++)
            {
                bool compact=count>3;int row=i/columns;int rowCount=Mathf.Min(columns,count-row*columns);
                var rt=sheets[i].GetComponent<RectTransform>();
                rt.sizeDelta=new Vector2(compact?285:350,compact?315:490);
                rt.anchoredPosition3D=new Vector3(((i%columns)-(rowCount-1)*.5f)*(compact?310:385),compact?175-row*350:0,0);
                var name=sheets[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>();
                if(name!=null){name.rectTransform.sizeDelta=new Vector2(compact?265:330,compact?120:135);name.rectTransform.anchoredPosition=new Vector2(0,compact?95:155);name.fontSize=compact?32:38;}
                var illustration=sheets[i].transform.Find("UIA2_Plate") as RectTransform;
                illustration.sizeDelta=new Vector2(compact?120:230,compact?90:180);illustration.anchoredPosition=Vector2.zero;
                var mastery=sheets[i].transform.Find("UIA3_CardMasteryText").GetComponent<UnityEngine.UI.Text>();
                mastery.rectTransform.sizeDelta=new Vector2(compact?265:330,65);mastery.rectTransform.anchoredPosition=new Vector2(0,compact?-80:-135);
                var track=sheets[i].transform.Find("UIA3_CardMastery") as RectTransform;
                track.sizeDelta=new Vector2(compact?255:310,22);track.anchoredPosition=new Vector2(0,compact?-132:-205);
                var fill=track.Find("Fill") as RectTransform;fill.sizeDelta=track.sizeDelta-new Vector2(8,8);
            }
            var dish=cooking?controller.OrderSession.ActiveOrder?.Dish:result?controller.OrderSession.LastResult?.Dish:null;
            dishRoot.SetActive(dish!=null);
            dishCollider.enabled=cooking&&dish!=null;
            var pointer = dishRoot.GetComponent<CounterDishPointer>();
            if (pointer != null)
            {
                pointer.interactable = cooking && dish != null;
                if (cooking && !wasCooking)
                {
                    var events = UnityEngine.EventSystems.EventSystem.current;
                    if (events != null) events.SetSelectedGameObject(dishRoot);
                }
            }
            wasCooking = cooking;
            var center=plate.bounds.center;
            dishRoot.transform.position=new Vector3(center.x,plate.bounds.max.y+.008f,center.z);
            sandwich.SetActive(dish!=null&&dish.DishId==1);
            mugCake.SetActive(dish!=null&&dish.DishId==2);
            genericDish.SetActive(dish!=null&&dish.DishId!=1&&dish.DishId!=2);
            bool focus=cooking||result;
            if(focus&&!focused)
            {
                returnRotation=sceneCamera.transform.rotation;returnFov=sceneCamera.fieldOfView;
                followWasEnabled=cursorFollow!=null&&cursorFollow.enabled;
                if(cursorFollow!=null)cursorFollow.enabled=false;
            }
            if(focus)
            {
                sceneCamera.transform.LookAt(dishRoot.transform.position+new Vector3(result ? .32f : 0f,.10f,0));
                sceneCamera.fieldOfView=40f;
            }
            else if(focused)RestoreCamera();
            if(board&&cursorFollow!=null&&cursorFollow.enabled)
                sceneCamera.transform.localRotation=cursorFollow.CursorRotation*Quaternion.Euler(0,boardYawBias,0);
            focused=focus;
        }
        private static bool HasSheetBindings(UnityEngine.UI.Button sheet)
        {
            if (sheet == null || sheet.transform.childCount == 0 ||
                sheet.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>() == null) return false;
            var mastery = sheet.transform.Find("UIA3_CardMasteryText");
            var track = sheet.transform.Find("UIA3_CardMastery");
            return sheet.transform.Find("UIA2_Plate") is RectTransform && mastery != null &&
                mastery.GetComponent<UnityEngine.UI.Text>() != null && track is RectTransform &&
                track.Find("Fill") is RectTransform;
        }
        private void RestoreCamera()
        {
            if(sceneCamera!=null){sceneCamera.transform.rotation=returnRotation;sceneCamera.fieldOfView=returnFov;}
            if(cursorFollow!=null)cursorFollow.enabled=followWasEnabled;
        }
        private void OnDisable()
        {
            if(!initialized)return;
            if(focused)RestoreCamera();focused=false;
            if(boardPaperLeft!=null)boardPaperLeft.enabled=leftWasEnabled;
            if(boardPaperRight!=null)boardPaperRight.enabled=rightWasEnabled;
            if(boardCanvas!=null)boardCanvas.gameObject.SetActive(false);
            if(dishRoot!=null)dishRoot.SetActive(false);
        }
    }
}
