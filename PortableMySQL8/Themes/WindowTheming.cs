using ControlzEx.Theming;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PortableMySQL8.Themes
{
	public class WindowTheming
	{
		public List<string> ValidAccents = new List<string>()
		{
			"Amber", "Blue", "Brown", "Cobalt", "Crimson", "Cyan", "Emerald",
			"Green", "Indigo", "Lime", "Magenta", "Mauve", "Olive", "Orange",
			"Pink", "Purple", "Red", "Sienna", "Steel", "Taupe", "Teal",
			"Violet", "Yellow"
		};

		public List<string> ValidThemes = new List<string>()
		{
			"Light", "Dark"
		};

		public WindowTheming()
		{

		}

		public void ApplyTheme(string accent, string theme)
		{
			if (!ValidAccents.Contains(accent))
				throw new InvalidAccentException(String.Format("The accent '{0}' is not valid!", accent));

			if (!ValidThemes.Contains(theme))
				throw new InvalidThemeException(String.Format("The theme '{0}' is not valid!", theme));

			ThemeManager.Current.ChangeTheme(Application.Current, theme + "." + accent);
		}
	}

	#region Exceptions

	public class InvalidAccentException : Exception
	{
		public InvalidAccentException()
		{

		}

		public InvalidAccentException(string message) : base(message)
		{

		}
	}

	public class InvalidThemeException : Exception
	{
		public InvalidThemeException()
		{

		}

		public InvalidThemeException(string message) : base(message)
		{

		}
	}

	#endregion Exceptions
}
