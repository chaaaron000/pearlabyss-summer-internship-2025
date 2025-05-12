using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class DiaryUIController : MonoBehaviour
{
    [SerializeField] private DiarySystem diarySystem;
    [SerializeField] private DiaryUIView diaryUIView;
    
    private UnityAction<int> showDiaryPageLambda;

    private void Awake()
    {
        showDiaryPageLambda = (page) => diaryUIView.ShowDiaryPage(
            page,
            diarySystem.GetCurrentDiarySprite(),
            diarySystem.GetCurrentDiaryContent());
    }

    private void OnEnable()
    {
        diarySystem = FindObjectOfType<DiarySystem>();
        if (diarySystem == null)
            Debug.LogError("FindObjectOfType<DiarySystem>() is null.");
        
        diaryUIView.Initialize(diarySystem.MaxDiaryNum);
        diaryUIView.leftButton.onClick.AddListener(OnClickLeftButton);
        diaryUIView.rightButton.onClick.AddListener(OnClickRightButton);
        diarySystem.OnDiaryModeChanged += diaryUIView.SetCanvasEnabled;
        diarySystem.OnScoreChanged += showDiaryPageLambda;

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        diaryUIView.leftButton.onClick.RemoveAllListeners();
        diaryUIView.rightButton.onClick.RemoveAllListeners();
        diarySystem.OnDiaryModeChanged -= diaryUIView.SetCanvasEnabled;
        diarySystem.OnScoreChanged -= showDiaryPageLambda;
        
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnClickLeftButton()
    {
        diarySystem.currentPageNum = diarySystem.currentPageNum - 1 < 0
            ? diarySystem.collectDiaryNum - 1
            : diarySystem.currentPageNum - 1;
        
        diaryUIView.ShowDiaryPage(
            diarySystem.currentPageNum + 1,
            diarySystem.GetCurrentDiarySprite(),
            diarySystem.GetCurrentDiaryContent());

        //일기장 넘기는 소리
        SoundManager.Instance.SFX_Play("diaryPageTurn");
    }

    private void OnClickRightButton()
    {
        diarySystem.currentPageNum = diarySystem.currentPageNum + 1 >= diarySystem.collectDiaryNum
            ? 0
            : diarySystem.currentPageNum + 1;
        
        diaryUIView.ShowDiaryPage(
            diarySystem.currentPageNum + 1,
            diarySystem.GetCurrentDiarySprite(),
            diarySystem.GetCurrentDiaryContent());

        //일기장 넘기는 소리
        SoundManager.Instance.SFX_Play("diaryPageTurn");
    }

    private void OnLocaleChanged(Locale newLocale)
    {
        diaryUIView.ShowDiaryPage(
            diarySystem.currentPageNum + 1,
            diarySystem.GetCurrentDiarySprite(),
            diarySystem.GetCurrentDiaryContent());
    }
}
