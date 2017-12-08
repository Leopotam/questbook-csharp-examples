using System.Collections.Generic;
using LeopotamGroup.Questbook;
using UnityEngine;

namespace LeopotamGroup.Examples.QuestbookTest {
    public class QuestbookTest : MonoBehaviour {
        QuestDocument _doc;

        readonly List<string> _texts = new List<string> ();

        readonly List<QuestChoice> _choices = new List<QuestChoice> ();

        void Start () {
            var markup = Resources.Load<TextAsset> ("quest1").text;

            // Next 2 methods will throw exceptions on any invalid data at markup, take care on it.
            _doc = QuestLoader.LoadMarkup (markup);
            QuestLoader.Validate (_doc);

            // Parsed document valid, we can start processing it.
            RenderPage ();
        }

        void RenderPage () {
            var page = _doc.GetCurrentPage ();
            if (page != null) {
                _texts.Clear ();
                _choices.Clear ();
                // Process optional page logics.
                if (page.logics != null) {
                    foreach (var logic in page.logics) {
                        _doc.ProcessLogic (logic);
                    }
                }
                // Process page text paragraphes.
                // Each paragraph can contains buitlin variables,
                // they should be processed with helper method of document.
                foreach (var text in page.texts) {
                    _texts.Add (_doc.ProcessText (text));
                }
                // If there is only one choice without text - process it automatically.
                if (_doc.MakeAutoChoice (page)) {
                    RenderPage ();
                    return;
                }
                // Process visible choices.
                foreach (var choice in page.choices) {
                    if (_doc.IsChoiceVisible (choice)) {
                        _choices.Add (choice);
                    }
                }
            } else {
                // Document completed.
                _texts.Add ("END");
            }
        }

        void OnGUI () {
            if (_doc != null) {
                GUILayout.BeginVertical (GUILayout.Width (400));

                // Render current page text paragraphes.
                foreach (var text in _texts) {
                    GUILayout.Label (text);
                }

                // Render current page choices.
                foreach (var choice in _choices) {
                    if (GUILayout.Button (choice.text)) {
                        // And process user input with redirect to new page.
                        _doc.MakeChoice (choice);
                        RenderPage ();
                        break;
                    }
                }

                if (_choices.Count == 0) {
                    if (GUILayout.Button ("Try again")) {
                        _doc.ResetProgress ();
                        RenderPage ();
                    }
                }

                GUILayout.EndVertical ();
            }
        }
    }
}