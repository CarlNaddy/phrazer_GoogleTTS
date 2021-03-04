
namespace Phrazer
{
    class SubsNewLineDetector
    {

        bool harvesterMode = false;

        public SubsNewLineDetector(bool harvesterModeParam)
        {
            harvesterMode = harvesterModeParam;
        }


        public int GetMaxBufferSize()
        {
            if(harvesterMode) return 180;
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
                harvesterMode &&
                (  textBuffer.EndsWith(".") || textBuffer.EndsWith("?") || textBuffer.EndsWith("!")  )
            ) return true;
            
            if(
                harvesterMode &&
                (  textBuffer.StartsWith("Trust me,")
                || textBuffer.StartsWith("trust me,")
                || textBuffer.StartsWith("By the way,")
                || textBuffer.StartsWith("by the way,")
                || textBuffer.StartsWith("I'm sorry,")
                || textBuffer.StartsWith("I don't know,")
                || textBuffer.StartsWith("You know,")
                || textBuffer.StartsWith("All right,")
                || textBuffer.StartsWith("No big deal,")
                )
            ) return true;

            if(
                !harvesterMode &&
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
                || textBuffer.EndsWith("___")
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
