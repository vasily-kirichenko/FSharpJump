using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;

namespace FSharpEditorEnhancements
{
    public class ToImage : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object ret = null;
            if (values.Length > 1)
            {
                var t = values[0] as FSharpJumpCore.JumpItem;
                var d = values[1] as System.Windows.FrameworkElement;
                String key = null;
                if (t.Keyword.IsNs)
                    key = "namespace";
                else if (t.Keyword.IsMod)
                    key = "module";
                else if (t.Keyword.IsLet)
                    key = "let";
                else if (t.Keyword.IsMember)
                    key = "member";
                else if (t.Keyword.IsStaticMember)
                    key = "staticMember";
                else if (t.Keyword.IsType)
                    key = "type";
                else
                    key = "marker";
                ret = d.Resources[key];
            }
            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ToShade : IMultiValueConverter
    {

        #region IMultiValueConverter Members

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object ret = null;
            if (values.Length > 1)
            {
                var t = values[0] as FSharpJumpCore.JumpItem;
                var d = values[1] as System.Windows.FrameworkElement;
                String key = null;
                switch (t.Level)
                {
                    case 0:
                    case 1:
                        key = "level1";
                        break;
                    case 2:
                        key = "level2";
                        break;
                    case 3:
                        key = "level3";
                        break;
                    default:
                        key = "level4";
                        break;
                }
                ret = d.Resources[key];
            }
            return ret;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ToStyle : IMultiValueConverter
    {

        #region IMultiValueConverter Members

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object ret = null;
            if (values.Length > 1)
            {
                var t = values[0] as FSharpJumpCore.JumpItem;
                var d = values[1] as System.Windows.FrameworkElement;
                String key = null;
                switch (t.Level)
                {
                    case 0:
                    case 1:
                        key = "s1";
                        break;
                    case 2:
                        key = "s2";
                        break;
                    case 3:
                        key = "s3";
                        break;
                    default:
                        key = "s4";
                        break;
                }
                ret = d.Resources[key];
            }
            return ret;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class WiderWidth : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int v = (int)value;
            return v * 4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
