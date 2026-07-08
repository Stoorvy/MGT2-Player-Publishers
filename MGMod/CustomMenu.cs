using System;
using System.Security.Policy;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static MGMod.SendPublisherOffer;

namespace MGMod
{
    public class CustomMenu
    {
        public Action<bool> MessageBoxResulted;

        private static mainScript mS;
        private static GUI_Main guiMain;
        private static sfxScript sfxScript_;

        //  244 => Menu_W_ServerDown
        //  350 => Menu_PublishingOfferVerhandlung
        //  17 => Menu_RenameRoom

        #region MessageBox
        public void CreateCustomMessageBox(string context, string title = "Stoorvy")
        {
            if (!sfxScript_)
            {
                sfxScript_ = GameObject.Find("SFX").GetComponent<sfxScript>();
                if (!sfxScript_) return;
            }
            if (!guiMain)
            {
                guiMain = GameObject.Find("CanvasInGameMenu").GetComponent<GUI_Main>();
                if (!guiMain) return;
            }

            //  244 => Menu_W_ServerDown
            GameObject box = GameObject.Instantiate(guiMain.uiObjects[244]);
            var msgBox = box.GetComponent<Menu_Messagebox>();
            var stupidIcon = box.transform.GetChild(0).GetChild(2);
            var acceptBut = box.transform.GetChild(0).GetChild(4).GetComponent<Button>();
            var cancelBut = box.transform.GetChild(0).GetChild(5).GetComponent<Button>();
            var closeBut = box.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetComponent<Button>();
            var titleText = box.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>();

            GameObject.Destroy(stupidIcon.gameObject);

            acceptBut.onClick.RemoveAllListeners();
            cancelBut.onClick.RemoveAllListeners();
            closeBut.onClick.RemoveAllListeners();

            var textUI = box.GetComponent<Menu_W_ServerDown>().uiObjects[0].GetComponent<Text>();

            box.transform.SetParent(guiMain.transform);

            box.SetActive(true);
            closeBut.gameObject.SetActive(false);
            acceptBut.gameObject.SetActive(true);
            cancelBut.gameObject.SetActive(true);
            textUI.gameObject.SetActive(true);

            box.transform.position = guiMain.uiObjects[244].transform.position;

            textUI.text = context;
            titleText.text = title;

            acceptBut.onClick.AddListener(() =>
            {
                MessageBoxResulted?.Invoke(true);
                sfxScript_.PlaySound(3, true);
                GameObject.Destroy(box);
            });
            cancelBut.onClick.AddListener(() =>
            {
                MessageBoxResulted?.Invoke(false);
                sfxScript_.PlaySound(3, true);
                GameObject.Destroy(box);
            });
        }
        #endregion

        #region PubOffer
        public void CreatePubOfferMenu(int gameID, int devID, int pubID, float gelenKar = 30f, int gelenGaranti = 100000)
        {
            if (!sfxScript_)
            {
                sfxScript_ = GameObject.Find("SFX").GetComponent<sfxScript>();
                if (!sfxScript_) return;
            }

            if (!guiMain)
            {
                guiMain = GameObject.Find("CanvasInGameMenu").GetComponent<GUI_Main>();
                if (!guiMain) return;
            }

            if (!mS)
            {
                mS = GameObject.Find("Main").GetComponent<mainScript>();
                if (!mS) return;
            }

            bool benDev = false;
            bool benPub = false;

            if (devID == mS.myID)
                benDev = true;
            if (pubID == mS.myID)
                benPub = true;

            if (!benDev && !benPub)
            {
                return;
            }

            gameScript theGame = null;

            foreach (var game in mS.games_.arrayGamesScripts)
            {
                if (game.myID == gameID)
                {
                    theGame = game;
                    break;
                }
            }

            if (theGame == null) return;

            if (theGame.isOnMarket) return;

            if(Offers.TryGetValue(new OfferKey(theGame.myID, pubID), out var offerData))
            {
                if (offerData.isOnMarket)
                {
                    return;
                }
            }

            GameObject menuObj = GameObject.Instantiate(guiMain.uiObjects[350]);
            GameObject inputFieldPrefab = guiMain.uiObjects[17].transform.GetChild(0).GetChild(3).gameObject;

            var menuScript = menuObj.GetComponent<MenuPublishingOfferVerhandlung>();

            var subTitleText = menuScript.uiObjects[0].GetComponent<Text>();
            var garantiText = menuScript.uiObjects[1].GetComponent<Text>();
            var garantiBG = menuScript.uiObjects[1].transform.parent.gameObject;

            var karText = menuScript.uiObjects[2].GetComponent<Text>();
            var karBG = menuScript.uiObjects[2].transform.parent.gameObject;

            var slider = menuScript.uiObjects[3];
            var memnuniyet = menuScript.uiObjects[4].transform.parent.gameObject;
            var teklifButton = menuScript.uiObjects[5].GetComponent<Button>();

            var kabulButton = menuObj.transform.GetChild(0).GetChild(7).GetComponent<Button>();
            var closeButton = menuObj.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(2).GetComponent<Button>();
            var senderIMG = menuObj.transform.GetChild(0).GetChild(5).GetChild(15).GetComponent<Image>();

            var gereksizButton = menuObj.transform.GetChild(0).GetChild(5).GetChild(7).gameObject;
            var gereksizButton2 = menuObj.transform.GetChild(0).GetChild(5).GetChild(8).gameObject;
            var gereksizButton3 = menuObj.transform.GetChild(0).GetChild(5).GetChild(9).gameObject;
            var gereksizButton4 = menuObj.transform.GetChild(0).GetChild(5).GetChild(10).gameObject;
            var gereksizButton5 = menuObj.transform.GetChild(0).GetChild(5).GetChild(11).gameObject;
            var gereksizButton6 = menuObj.transform.GetChild(0).GetChild(5).GetChild(12).gameObject;
            var gereksizIMG = menuObj.transform.GetChild(0).GetChild(5).GetChild(14).gameObject;
            var gereksizIMG2 = menuObj.transform.GetChild(0).GetChild(5).GetChild(16).gameObject;

            UnityEngine.Object.DestroyImmediate(menuScript);

            GameObject.Destroy(slider);
            GameObject.Destroy(memnuniyet);
            GameObject.Destroy(gereksizIMG);
            GameObject.Destroy(gereksizIMG2);
            GameObject.Destroy(gereksizButton);
            GameObject.Destroy(gereksizButton2);
            GameObject.Destroy(gereksizButton3);
            GameObject.Destroy(gereksizButton4);
            GameObject.Destroy(gereksizButton5);
            GameObject.Destroy(gereksizButton6);

            //----------------------------------------------------
            // Custom Script
            //----------------------------------------------------

            var customMenuScript = menuObj.AddComponent<CustomOfferMenu>();

            customMenuScript.gS = theGame;
            customMenuScript.defGaranti = gelenGaranti;
            customMenuScript.defKar = gelenKar;
            if (pubID == mS.myID)
                customMenuScript.kabulButton = kabulButton;
            
            //----------------------------------------------------
            // Garanti InputField
            //----------------------------------------------------

            garantiText.gameObject.SetActive(false);

            GameObject garantiInputObj = GameObject.Instantiate(inputFieldPrefab, garantiBG.transform);

            RectTransform garantiRect = garantiInputObj.GetComponent<RectTransform>();
            garantiRect.anchorMin = Vector2.zero;
            garantiRect.anchorMax = Vector2.one;
            garantiRect.offsetMin = Vector2.zero;
            garantiRect.offsetMax = Vector2.zero;
            garantiRect.localScale = Vector3.one;

            InputField garantiInput = garantiInputObj.GetComponent<InputField>();
            garantiInput.text = FormatGuarantee(gelenGaranti.ToString());

            Text garantiPlaceholder = garantiInput.placeholder.GetComponent<Text>();
            garantiPlaceholder.text = "";

            garantiInput.textComponent.alignment = TextAnchor.MiddleCenter;
            garantiInput.contentType = InputField.ContentType.IntegerNumber;

            garantiInput.textComponent.gameObject.SetActive(true);
            garantiInput.textComponent.color = Color.black;

            garantiInput.onEndEdit.AddListener((_) =>
            {
                if (string.IsNullOrEmpty(garantiInput.text))
                {
                    garantiInput.contentType = InputField.ContentType.Standard;
                    garantiInput.text = FormatGuarantee(gelenGaranti.ToString());
                    garantiInput.textComponent.text = FormatGuarantee(gelenGaranti.ToString());
                    garantiInput.contentType = InputField.ContentType.IntegerNumber;
                }
                else
                {
                    garantiInput.contentType = InputField.ContentType.Standard;
                    garantiInput.text = FormatGuarantee(garantiInput.text);
                    garantiInput.textComponent.text = FormatGuarantee(garantiInput.text);
                    garantiInput.contentType = InputField.ContentType.IntegerNumber;
                }
            });

            garantiInput.textComponent.resizeTextForBestFit = true;

            customMenuScript.garantiField = garantiInput;

            //----------------------------------------------------
            // Kar Oranı InputField
            //----------------------------------------------------

            karText.gameObject.SetActive(false);

            GameObject karInputObj = GameObject.Instantiate(inputFieldPrefab, karBG.transform);

            RectTransform karRect = karInputObj.GetComponent<RectTransform>();
            karRect.anchorMin = Vector2.zero;
            karRect.anchorMax = Vector2.one;
            karRect.offsetMin = Vector2.zero;
            karRect.offsetMax = Vector2.zero;
            karRect.localScale = Vector3.one;

            InputField karInput = karInputObj.GetComponent<InputField>();
            karInput.text = FormatProfit(gelenKar.ToString());

            Text karPlaceholder = karInput.placeholder.GetComponent<Text>();
            karPlaceholder.text = "";

            karInput.textComponent.alignment = TextAnchor.MiddleCenter;
            karInput.contentType = InputField.ContentType.DecimalNumber;

            karInput.textComponent.gameObject.SetActive(true);
            karInput.textComponent.color = Color.black;

            karInput.onEndEdit.AddListener((_) =>
            {
                if (string.IsNullOrEmpty(karInput.text))
                {
                    karInput.contentType = InputField.ContentType.Standard;
                    karInput.text = FormatProfit(gelenKar.ToString());
                    karInput.textComponent.text = FormatProfit(gelenKar.ToString());
                    karInput.contentType = InputField.ContentType.DecimalNumber;
                }
                else
                {
                    karInput.contentType = InputField.ContentType.Standard;
                    karInput.text = FormatProfit(karInput.text);
                    karInput.textComponent.text = FormatProfit(karInput.text);
                    karInput.contentType = InputField.ContentType.DecimalNumber;
                }
            });

            karInput.textComponent.resizeTextForBestFit = true;

            customMenuScript.karField = karInput;

            //----------------------------------------------------
            // Subtitle
            //----------------------------------------------------

            if (benDev)
            {
                publisherScript pS_ = null;

                foreach (var publisher in mS.arrayPublisherScripts)
                {
                    if (publisher.myID == pubID)
                    {
                        pS_ = publisher;
                        break;
                    }
                }

                if (pS_ == null) { GameObject.Destroy(menuObj); return; }

                subTitleText.text = $"Make an offer with <color=blue>{pS_.GetName()}</color> to publish <color=blue>{theGame.GetNameSimple()}</color>!";

                senderIMG.sprite = pS_.GetLogo();
            }
            if (benPub)
            {
                publisherScript pS_ = null;

                foreach (var publisher in mS.arrayPublisherScripts)
                {
                    if (publisher.myID == devID)
                    {
                        pS_ = publisher;
                        break;
                    }
                }

                if (pS_ == null) { GameObject.Destroy(menuObj); return; }

                subTitleText.text = $"<color=blue>{pS_.GetName()}</color> sent an offer to publish <color=blue>{theGame.GetNameSimple()}</color>!";

                senderIMG.sprite = pS_.GetLogo();
            }

            //----------------------------------------------------
            // Butonlar
            //----------------------------------------------------

            teklifButton.GetComponentInChildren<Text>().text = "Send Offer";
            kabulButton.GetComponentInChildren<Text>().text = "Accept Offer";

            teklifButton.onClick.RemoveAllListeners();
            kabulButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();

            bool kabulActive = false;
            if (benPub)
            {
                kabulActive = true;
            }
            else if (benDev && benPub && MGMod.DebugMode.Value)
            {
                kabulActive = true;
            }

            if (!kabulActive)
            {
                kabulButton.gameObject.SetActive(false);
            }

            teklifButton.interactable = true;

            teklifButton.onClick.AddListener(() =>
            {
                int garanti = (int)GetGuaranteeValue(garantiInput);
                float kar = GetProfitValue(karInput);

                theGame.SetPublisher(pubID);

                if (theGame.developerID == mS.myID)
                {
                    if (mS.mpCalls_.isServer)
                    {
                        mS.mpCalls_.SERVER_Send_Game(theGame);
                    }
                    else if (mS.mpCalls_.isClient)
                    {
                        mS.mpCalls_.CLIENT_Send_Game(theGame);
                    }
                }

                bool gelenMi = false;
                if (MGMod.DebugMode.Value && theGame.developerID == mS.myID && pubID == mS.myID)
                    gelenMi = true;

                var offerData = new OfferData
                {
                    gameID = theGame.myID,
                    developerID = theGame.developerID,
                    publisherID = pubID,
                    kar = kar,
                    garanti = garanti,
                    gelenMi = gelenMi
                };

                SendPublisherOffer.SetOfferData(offerData, new OfferKey(theGame.myID, pubID));

                SendPublisherOffer.SendPublishOfferMessage(theGame, mS.mpCalls_.isServer, pubID, kar, garanti);

                Debug.Log($"Teklif gönderildi Garanti={garanti} Kar={kar}%");
                sfxScript_.PlaySound(3, true);
                GameObject.Destroy(menuObj);
            });

            kabulButton.onClick.AddListener(() =>
            {
                theGame.SetPublisher(pubID);
                theGame.pubAngebot_Gewinnbeteiligung = gelenKar;

                int payment = gelenGaranti;
                if (mS.mpCalls_.isServer)
                {
                    mS.mpCalls_.SERVER_Send_Help(mS.myID, devID, 0, payment, -1, -1);
                }
                else if (mS.mpCalls_.isClient)
                {
                    mS.mpCalls_.CLIENT_Send_Help(devID, 0, payment, -1, -1);
                }

                var guiMain_ = GameObject.Find("CanvasInGameMenu")?.GetComponent<GUI_Main>();
                guiMain_.ActivateMenu(guiMain_.uiObjects[218]);
                guiMain_.uiObjects[218].GetComponent<Menu_Packung>().Init(theGame, null, true, true);

                if (benDev && benPub && MGMod.DebugMode.Value && Offers.TryGetValue(new OfferKey(theGame.myID, pubID), out var offer))
                {
                    if (offer.task != null)
                    {
                        UnityEngine.Object.Destroy(offer.task.gameObject);
                    }
                }

                mS.publishingOfferMain_.amountPublishingOffers--;
                mS.publishingOfferMain_.UpdateGUI();

                Debug.Log($"Teklif kabul edildi Garanti={gelenGaranti} Kar={gelenKar}%");
                sfxScript_.PlaySound(3, true);
                GameObject.Destroy(menuObj);
            });

            closeButton.onClick.AddListener(() =>
            {
                sfxScript_.PlaySound(3, true);
                GameObject.Destroy(menuObj);
            });

            menuObj.transform.SetParent(guiMain.transform, false);
            menuObj.SetActive(true);
        }

        private static string FormatGuarantee(string input)
        {
            string digits = "";

            foreach (char c in input)
            {
                if (char.IsDigit(c))
                    digits += c;
            }

            if (digits.Length == 0)
                digits = "0";

            long value = long.Parse(digits);
            
            return value.ToString("N0") + mS?.tS_.GetText(7);
        }

        private static long GetGuaranteeValue(InputField field)
        {
            string digits = "";

            foreach (char c in field.text)
            {
                if (char.IsDigit(c))
                    digits += c;
            }

            if (digits.Length == 0)
                return 0;

            return long.Parse(digits);
        }

        private static string FormatProfit(string input)
        {
            string result = "";
            bool dot = false;

            foreach (char c in input)
            {
                if (char.IsDigit(c))
                    result += c;
                else if ((c == '.' || c == ',') && !dot)
                {
                    result += '.';
                    dot = true;
                }
            }

            if (result.Length == 0)
                result = "0";

            float value;

            if (!float.TryParse(result,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out value))
            {
                value = 0;
            }

            value = Mathf.Clamp(value, 0, 100);

            return value.ToString("0.##") + "%";
        }

        private static float GetProfitValue(InputField field)
        {
            string text = field.text.Replace("%", "").Replace(",", ".");

            float.TryParse(text,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out float value);

            return Mathf.Clamp(value, 0, 100);
        }

        public class CustomOfferMenu : MonoBehaviour
        {
            public gameScript gS;

            public InputField garantiField;
            public InputField karField;
            public Button kabulButton;

            private bool garatiOK;
            private bool karOK;

            public int defGaranti;
            public float defKar;

            void Update()
            {
                if (gS != null)
                {
                    if (gS.isOnMarket)
                    {
                        GameObject.Destroy(gameObject);
                    }
                }
                if (garantiField != null && !garatiOK)
                {
                    garantiField.onEndEdit.AddListener(OnGarantiEdit);
                    garatiOK = true;
                }
                if (karField != null && !karOK)
                {
                    karField.onEndEdit.AddListener(OnKarEdit);
                    karOK = true;
                }
            }

            bool isDef()
            {
                if(garantiField.text == defGaranti.ToString() || garantiField.text == FormatGuarantee(defGaranti.ToString()))
                {
                    if(karField.text == defKar.ToString() || karField.text == FormatProfit(defKar.ToString()))
                        return true;
                }
                return false;
            }

            void OnGarantiEdit(string txt)
            {
                if (!isDef())
                {
                    if (kabulButton != null)
                    {
                        kabulButton.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (kabulButton != null)
                    {
                        kabulButton.gameObject.SetActive(true);
                    }
                }
            }

            void OnKarEdit(string txt)
            {
                if (!isDef())
                {
                    if (kabulButton != null)
                    {
                        kabulButton.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (kabulButton != null)
                    {
                        kabulButton.gameObject.SetActive(true);
                    }
                }
            }
        }
        #endregion

    }
}
