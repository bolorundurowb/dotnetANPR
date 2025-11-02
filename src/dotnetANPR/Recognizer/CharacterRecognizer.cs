using System.Collections.Generic;
using System.IO;
using DotNetANPR.Config;
using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.Recognizer
{
    public abstract class CharacterRecognizer
    {
        protected Dictionary<char, float[]> _alphabet;
        protected AppSettings _settings;

        protected CharacterRecognizer(AppSettings settings)
        {
            _settings = settings;
            LoadAlphabet();
        }

        private void LoadAlphabet()
        {
            _alphabet = new Dictionary<char, float[]>();
            string alphabetPath = _settings.Recognition.CharLearnAlphabetPath;
            if (!Directory.Exists(alphabetPath))
            {
                throw new DirectoryNotFoundException($"Alphabet path not found: {alphabetPath}");
            }

            foreach (var file in Directory.GetFiles(alphabetPath, "*.jpg"))
            {
                char character = Path.GetFileNameWithoutExtension(file)[0];
                using var photo = new Photo(file);
                using var charPhoto = new LicensePlateChar(photo, null, _settings);
                _alphabet.Add(character, charPhoto.GetFeatureVector());
            }
        }
    }
}