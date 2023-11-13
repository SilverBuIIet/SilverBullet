﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Tesseract;

namespace RuriLib.ViewModels
{
    /// <summary>
    /// Tesseract set variable value type
    /// </summary>
    public enum VariableValueType
    {
        /// <summary>String</summary>
        String,
        /// <summary>Int</summary>
        Integer,
        /// <summary>Double</summary>
        Double,
        /// <summary>Boolean</summary>
        Boolean
    }

    /// <summary>
    /// Tesseract variable
    /// </summary>
    public class TesseractVariable
    {
        /// <summary>
        /// variable name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// variable value
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// variable type
        /// </summary>
        public VariableValueType ValueType { get; set; }
        /// <summary>
        /// name:value:type
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name}:{Value}:{ValueType}";
        }
    }

    /// <summary>
    /// Provides ocr-related settings.
    /// </summary>
    public class SettingsOcr : ViewModelBase
    {
        private bool saveImageToCaptchasFolder;
        /// <summary>
        /// Save image to captcha folder TRUE or FALSE
        /// </summary>
        public bool SaveImageToCaptchasFolder { get { return saveImageToCaptchasFolder; } set { saveImageToCaptchasFolder = value; OnPropertyChanged(); } }

        private bool getIterator;
        public bool GetIterator { get { return getIterator; } set { getIterator = value; OnPropertyChanged(); } }

        private ObservableCollection<TesseractVariable> variableList = new ObservableCollection<TesseractVariable>();
        /// <summary>
        /// var list
        /// </summary>
        public ObservableCollection<TesseractVariable> VariableList { get { return variableList; } set { variableList = value; OnPropertyChanged(); } }

        /// <summary>
        /// Resets the properties to their default value.
        /// </summary>
        public void Reset()
        {
            SettingsOcr def = new SettingsOcr();
            var props = new List<PropertyInfo>(typeof(SettingsOcr).GetProperties());

            foreach (PropertyInfo prop in props)
            {
                prop.SetValue(this, prop.GetValue(def, null));
            }
        }

    }
}
