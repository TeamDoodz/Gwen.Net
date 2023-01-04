using System;
using System.Linq;
using Gwen.Net.Control;
using Gwen.Net.Platform;
using Gwen.Net.Xml;
using static Gwen.Net.Platform.GwenPlatform;

namespace Gwen.Net.CommonDialog;

/// <summary>
/// Base class for a file or directory dialog.
/// </summary>
public abstract class FileDialog : Component {
	private Action<string?>? callback;

	private string currentFolder = "C:/";
	private string currentFilter = "*";

	private bool foldersOnly;

	private bool onClosing;

	private TreeControl? folders;
	private ListBox? items;
	private TextBox? path;
	private TextBox? selectedName;
	private ComboBox? filters;
	private Button? ok;
	private Button? newFolder;
	private VerticalSplitter? nameFilterSplitter;
	private Label? fileNameLabel;
	private Window? window;

	/// <summary>
	/// Initial folder for the dialog.
	/// </summary>
	public string InitialFolder { set { SetPath(value); } }

	/// <summary>
	/// Set initial folder and selected item.
	/// </summary>
	public string CurrentItem { set { SetPath(GetDirectoryName(value) ?? "C:/"); SetCurrentItem(GetFileName(value) ?? "C:/"); } }

	/// <summary>
	/// Window title.
	/// </summary>
	public string Title { 
		get => window?.Title ?? "";
		set {
			if(window != null) {
				window.Title = value;
			}
		} 
	}

	/// <summary>
	/// File filters. See <see cref="SetFilters(string, int)"/>.
	/// </summary>
	public string Filters { set { SetFilters(value); } }

	/// <summary>
	/// Text shown in the ok button.
	/// </summary>
	public string OkButtonText { 
		get => ok?.Text ?? "";
		set {
			if(ok != null) {
				ok.Text = value;
			}
		} 
	}

	/// <summary>
	/// Function that is called when dialog is closed. If ok is pressed, parameter is the selected file / directory.
	/// If cancel is pressed or window closed, parameter is null.
	/// </summary>
	public Action<string?>? Callback { get { return callback; } set { callback = value; } }

	/// <summary>
	/// Hide or show new folder button.
	/// </summary>
	public bool EnableNewFolder { 
		get => !newFolder?.IsCollapsed ?? false;
		set {
			if(newFolder != null) {
				newFolder.IsCollapsed = !value;
			}
		} 
	}

	/// <summary>
	/// Show only directories.
	/// </summary>
	protected bool FoldersOnly {
		get { return foldersOnly; }
		set {
			foldersOnly = value;
			if(filters != null) {
				filters.IsCollapsed = value;
			}
			if(fileNameLabel != null) {
				fileNameLabel.Text = "Folder name:";
			}
			if(nameFilterSplitter != null) {
				if(value) {
					nameFilterSplitter.Zoom(0);
				} else {
					nameFilterSplitter.UnZoom();
				}
			}
		}
	}

	/// <summary>
	/// Constructor for the base class. Implementing classes must call this.
	/// </summary>
	/// <param name="parent">Parent.</param>
	protected FileDialog(ControlBase parent)
		: base(parent, new XmlStringSource(Xml)) {
	}

	protected override void OnCreated() {
		window = View as Window;
		folders = GetControl<TreeControl>("Folders");
		items = GetControl<ListBox>("Items");
		path = GetControl<TextBox>("Path");
		selectedName = GetControl<TextBox>("SelectedName");
		filters = GetControl<ComboBox>("Filters");
		ok = GetControl<Button>("Ok");
		newFolder = GetControl<Button>("NewFolder");
		nameFilterSplitter = GetControl<VerticalSplitter>("NameFilterSplitter");
		fileNameLabel = GetControl<Label>("FileNameLabel");

		UpdateFolders();

		onClosing = false;

		currentFolder = CurrentDirectory;

		currentFilter = "*.*";
		filters?.AddItem("All files (*.*)", "All files (*.*)", "*.*");
	}

	/// <summary>
	/// Set current path.
	/// </summary>
	/// <param name="path">Path.</param>
	/// <returns>True if the path change was successful. False otherwise.</returns>
	public bool SetPath(string path) {
		if(DirectoryExists(path)) {
			currentFolder = path;
			if(this.path != null) {
				this.path.Text = currentFolder;
			}
			UpdateItemList();
			return true;
		}

		return false;
	}

	/// <summary>
	/// Set filters.
	/// </summary>
	/// <param name="filterStr">Filter string. Format 'name|filter[|name|filter]...'</param>
	/// <param name="current">Set this index as a current filter.</param>
	public void SetFilters(string filterStr, int current = 0) {
		string[] filters = filterStr.Split('|');
		if((filters.Length & 0x1) == 0x1)
			throw new Exception("Error in filter.");

		if(this.filters != null) {
			this.filters.RemoveAll();

			for(int i = 0; i < filters.Length; i += 2) {
				this.filters.AddItem(filters[i], filters[i], filters[i + 1]);
			}

			this.filters.SelectedIndex = current;
		}
	}

	/// <summary>
	/// Set current file or directory.
	/// </summary>
	/// <param name="item">File or directory. This doesn't need to exists.</param>
	protected void SetCurrentItem(string item) {
		if(selectedName != null) {
			selectedName.Text = item;
		}
	}

	/// <summary>
	/// Close the dialog and call the call back function.
	/// </summary>
	/// <param name="path">Parameter for the call back function.</param>
	protected void Close(string? path) {
		OnClosing(path, true);
	}

	/// <summary>
	/// Called when the user selects a file or directory.
	/// </summary>
	/// <param name="path">Full path of selected file or directory.</param>
	protected virtual void OnItemSelected(string path) {
		if((DirectoryExists(path) && foldersOnly) || (FileExists(path) && !foldersOnly)) {
			SetCurrentItem(GetFileName(path));
		}
	}

	/// <summary>
	/// Called to validate the file or directory name when the user enters it.
	/// </summary>
	/// <param name="path">Full path of the name.</param>
	/// <returns>Is the name valid.</returns>
	protected virtual bool IsSubmittedNameOk(string path) {
		if(DirectoryExists(path)) {
			if(!foldersOnly) {
				SetPath(path);
			}
		} else if(FileExists(path)) {
			return true;
		} else {
			return true;
		}

		return false;
	}

	/// <summary>
	/// Called to validate the path when the user presses the ok button.
	/// </summary>
	/// <param name="path">Full path.</param>
	/// <returns>Is the path valid.</returns>
	protected virtual bool ValidateFileName(string path) {
		return true;
	}

	/// <summary>
	/// Called when the dialog is closing.
	/// </summary>
	/// <param name="path">Path for the call back function</param>
	/// <param name="doClose">True if the dialog needs to be closed.</param>
	protected virtual void OnClosing(string? path, bool doClose) {
		if(onClosing)
			return;

		onClosing = true;

		if(doClose)
			window?.Close();

		callback?.Invoke(path);
	}

	private void OnPathSubmitted(ControlBase sender, EventArgs args) {
		if(path != null && !SetPath(path.Text)) {
			path.Text = currentFolder;
		}
	}

	private void OnUpClicked(ControlBase sender, ClickedEventArgs args) {
		string? newPath = GetDirectoryName(currentFolder);
		if(newPath != null) {
			SetPath(newPath);
		}
	}

	private void OnNewFolderClicked(ControlBase sender, ClickedEventArgs args) {
		if(this.path == null) return;

		string path = this.path.Text;
		if(DirectoryExists(path)) {
			this.path.Focus();
		} else {
			try {
				CreateDirectory(path);
				SetPath(path);
			} catch(Exception ex) {
				MessageBox.Show(View, ex.Message, Title, MessageBoxButtons.OK);
			}
		}
	}

	private void OnFolderSelected(ControlBase sender, EventArgs args) {
		if(sender is TreeNode node && node.UserData is string userDataString) {
			SetPath(userDataString);
		}
	}

	private void OnItemSelected(ControlBase sender, ItemSelectedEventArgs args) {
		if(args.SelectedItem?.UserData is string path) {
			OnItemSelected(path);
		}
	}

	private void OnItemDoubleClicked(ControlBase sender, ItemSelectedEventArgs args) {
		if(args.SelectedItem?.UserData is string path) {
			if(DirectoryExists(path)) {
				SetPath(path);
			} else {
				OnOkClicked(null!, new ClickedEventArgs(0, 0, true));
			}
		}
	}

	private void OnNameSubmitted(ControlBase sender, EventArgs args) {
		string path = Combine(currentFolder, selectedName?.Text ?? "");
		if(IsSubmittedNameOk(path))
			OnOkClicked(null!, new ClickedEventArgs(0, 0, true));
	}

	private void OnFilterSelected(ControlBase sender, ItemSelectedEventArgs args) {
		currentFilter = filters?.SelectedItem?.UserData as string ?? "*";
		UpdateItemList();
	}

	private void OnOkClicked(ControlBase sender, ClickedEventArgs args) {
		string path = Combine(currentFolder, selectedName?.Text ?? "");
		if(ValidateFileName(path)) {
			OnClosing(path, true);
		}
	}

	private void OnCancelClicked(ControlBase sender, ClickedEventArgs args) {
		OnClosing(null, true);
	}

	private void OnWindowClosed(ControlBase sender, EventArgs args) {
		OnClosing(null, false);
	}

	private void UpdateItemList() {
		if(items == null) {
			return;
		}

		items.Clear();

		IOrderedEnumerable<IFileSystemDirectoryInfo>? directories;
		IOrderedEnumerable<IFileSystemFileInfo>? files = null;
		try {
			directories = GetDirectories(currentFolder).OrderBy(di => di.Name);
			if(!foldersOnly)
				files = GetFiles(currentFolder, currentFilter).OrderBy(fi => fi.Name);
		} catch(Exception ex) {
			MessageBox.Show(View, ex.Message, Title, MessageBoxButtons.OK);
			return;
		}

		foreach(IFileSystemDirectoryInfo di in directories) {
			ListBoxRow row = items.AddRow(di.Name, "", di.FullName);
			row.SetCellText(1, "<dir>");
			row.SetCellText(2, di.FormattedLastWriteTime);
		}

		if(!foldersOnly && files != null) {
			foreach(IFileSystemFileInfo fi in files) {
				ListBoxRow row = items.AddRow(fi.Name, "", fi.FullName);
				row.SetCellText(1, fi.FormattedFileLength);
				row.SetCellText(2, fi.FormattedFileLength);
			}
		}
	}

	private void UpdateFolders() {
		if(folders == null) {
			return;
		}

		folders.RemoveAllNodes();

		foreach(ISpecialFolder folder in Platform.GwenPlatform.GetSpecialFolders()) {
			TreeNode category = folders.FindNodeByName(folder.Category, false) ?? folders.AddNode(folder.Category, folder.Category, null);

			category.AddNode(folder.Name, folder.Name, folder.Path);
		}

		folders.ExpandAll();
	}

	private string FormatFileLength(long length) {
		if(length > 1024 * 1024 * 1024)
			return String.Format("{0:0.0} GB", (double)length / (1024 * 1024 * 1024));
		else if(length > 1024 * 1024)
			return String.Format("{0:0.0} MB", (double)length / (1024 * 1024));
		else if(length > 1024)
			return String.Format("{0:0.0} kB", (double)length / 1024);
		else
			return String.Format("{0} B", length);
	}

	private string FormatFileTime(DateTime dateTime) {
		return "";
		//return String.Format("{0} {1}", dateTime.ToShortDateString(), dateTime.ToLongTimeString());
	}

	private static string Xml => @"<?xml version='1.0' encoding='UTF-8'?>
			<Window Size='400,300' StartPosition='CenterCanvas' Closed='OnWindowClosed'>
				<DockLayout Margin='2' >
					<DockLayout Dock='Top'>
						<Label Dock='Left' Margin='2' Alignment='CenterV,Left' Text='Path:' />
						<TextBox Name='Path' Margin='2' Dock='Fill' SubmitPressed='OnPathSubmitted' />
						<Button Name='NewFolder' Margin='2' Dock='Right' Padding='10,0,10,0' Text='New' Clicked='OnNewFolderClicked' />
						<Button Name='Up' Margin='2' Dock='Right' Padding='10,0,10,0' Text='Up' Clicked='OnUpClicked' />
					</DockLayout>
					<VerticalSplitter Dock='Fill' Value='0.3' SplitterSize='2'>
						<TreeControl Name='Folders' Margin='2' Selected='OnFolderSelected' />
						<ListBox Name='Items' Margin='2' ColumnCount='3' RowSelected='OnItemSelected' RowDoubleClicked='OnItemDoubleClicked' />
					</VerticalSplitter>
					<DockLayout Dock='Bottom'>
						<Button Name='Cancel' Margin='2' Dock='Right' Padding='10,0,10,0' Width='100' Text='Cancel' Clicked='OnCancelClicked' />
						<Button Name='Ok' Margin='2' Dock='Right' Padding='10,0,10,0' Width='100' Text='Ok' Clicked='OnOkClicked' />
					</DockLayout>
					<VerticalSplitter Name='NameFilterSplitter' Dock='Bottom' Value='0.7' SplitterSize='2'>
						<DockLayout>
							<Label Name='FileNameLabel' Dock='Left' Margin='2' Alignment='CenterV,Left' Text='File name:' />
							<TextBox Name='SelectedName' Dock='Fill' Margin='2' SubmitPressed='OnNameSubmitted'/>
						</DockLayout>
						<ComboBox Name='Filters' Margin='2' ItemSelected='OnFilterSelected'/>
					</VerticalSplitter>
				</DockLayout>
			</Window>
			";
}