
namespace Phrazer
{
    class SubsNewLineDetector
    {

        bool phrazeMode = false;

        public SubsNewLineDetector(bool phrazeModeParam)
        {
            phrazeMode = phrazeModeParam;
        }


        public int GetMaxBufferSize()
        {
            if(phrazeMode) return 180;
            return 80;
        }
        

        public bool ShouldCreateNewRow(string textBuffer, string word)
        {
            // Experimantal end of phrase recognition feature
            if(
                EndOfPhraseDetected(textBuffer, word)
                && !textBuffer.EndsWith("Mr.")
                && !textBuffer.EndsWith("Mrs.")
                && !textBuffer.EndsWith("Dr.")
                && !textBuffer.EndsWith("U.S.")
                && !textBuffer.EndsWith("L.A.")
                && !textBuffer.EndsWith("K.O.")
                && !textBuffer.EndsWith("C.E.O.")
                && textBuffer.Length > 2
            ) return true;

            return false;
        }

        bool EndOfPhraseDetected(string textBuffer, string word)
        {
            // THE REALLY STRICT MODE
            
            if(
                phrazeMode &&
                (  textBuffer.EndsWith(".") || textBuffer.EndsWith("?") || textBuffer.EndsWith("!")  )
            ) return true;
            

            if(
                phrazeMode &&
                (  textBuffer.StartsWith("Wow,")
                || textBuffer.StartsWith("wow,")
                || textBuffer.StartsWith("Whoa,")

                || textBuffer.StartsWith("Ah,")
                || textBuffer.StartsWith("ah,")
                || textBuffer.StartsWith("Oh,")
                || textBuffer.StartsWith("oh,")
                || textBuffer.StartsWith("Uh,")
                || textBuffer.StartsWith("uh,")

                || textBuffer.StartsWith("Okay,")
                || textBuffer.StartsWith("okay,")

                || textBuffer.StartsWith("Well,")
                || textBuffer.StartsWith("well,")

                || textBuffer.StartsWith("So,")
                || textBuffer.StartsWith("so,")

                || textBuffer.StartsWith("Yeah,")
                || textBuffer.StartsWith("yeah,")

                || textBuffer.StartsWith("Yes,")
                || textBuffer.StartsWith("yes,")
                || textBuffer.StartsWith("No,")
                || textBuffer.StartsWith("no,")

                || textBuffer.StartsWith("Now,")
                || textBuffer.StartsWith("now,")

                || textBuffer.StartsWith("Look,")

                || textBuffer.StartsWith("And,")
                || textBuffer.StartsWith("and,")
                                
                || textBuffer.StartsWith("But,")
                || textBuffer.StartsWith("but,")

                || textBuffer.StartsWith("Trust me,")
                || textBuffer.StartsWith("trust me,")

                || textBuffer.StartsWith("Listen,")
                || textBuffer.StartsWith("Frankie,")

                || textBuffer.StartsWith("Darling,")
                || textBuffer.StartsWith("Sweetheart,")

                || textBuffer.StartsWith("Alan,")
                || textBuffer.StartsWith("Charlie,")
                || textBuffer.StartsWith("Judith,")
                || textBuffer.StartsWith("Rose,")
                || textBuffer.StartsWith("Jake,")
                || textBuffer.StartsWith("Berta,")
                || textBuffer.StartsWith("Lyndsey,")
                || textBuffer.StartsWith("Mom,")
                )
            ) return true;


            if(
                !phrazeMode &&
                (  textBuffer.Length > 30 && textBuffer.EndsWith(",") && !word.EndsWith(".") && !word.EndsWith("?") && !word.EndsWith("!")
                || textBuffer.Length > 30 && textBuffer.EndsWith("-")
                || textBuffer.Length > 60 && word.Trim() == "to"
                || textBuffer.Length > 60 && word.Trim() == "that"
                || textBuffer.Length > 60 && word.Trim() == "and"
                || textBuffer.Length > 60 && word.Trim() == "of"
                )
            ) return true;

            

            /*
            if(textBuffer.EndsWith(",") || textBuffer.EndsWith(".")) {
                if(textBuffer.Split(' ').Length < 2) {
                    File.AppendAllText(GetOutputAbsoluteFilename("__CUTOFFLIST"), textBuffer + Environment.NewLine, Encoding.UTF8);
                }
            }
            */

            if(
                /* End Of Phrase Detected */
                textBuffer.Length > 5 && textBuffer.EndsWith(".")
                || textBuffer.Length > 5 && textBuffer.EndsWith(":")
                || textBuffer.Length > 5 && textBuffer.EndsWith("?")
                || textBuffer.Length > 5 && textBuffer.EndsWith("!")
                || textBuffer.EndsWith(")")
                || textBuffer.EndsWith("]")
                || textBuffer.EndsWith("--")
                || textBuffer.EndsWith("♪")
                || textBuffer.Contains("NETFLIX")
                /* StartingWordDetected */
                || word.Trim() == "Why"
                || word.Trim() == "What"
                || word.Trim() == "What's"
                || word.Trim() == "We're"
                || word.Trim() == "The"
                || word.Trim() == "Then"
                || word.Trim() == "There"
                || word.Trim() == "To"
                || word.Trim() == "In"
                || word.Trim() == "So"
                || word.Trim() == "Perhaps"
                || word.StartsWith(">>")
            ) return true;

            return false;   
        }

        
    }
}
