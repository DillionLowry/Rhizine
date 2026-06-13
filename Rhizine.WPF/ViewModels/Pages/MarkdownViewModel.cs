using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit.Document;
using Rhizine.WPF.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhizine.WPF.ViewModels.Pages
{
    public class MarkdownPage
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public ObservableCollection<MarkdownSubpage> Subpages { get; set; }

        public MarkdownPage()
        {
            Subpages = new ObservableCollection<MarkdownSubpage>();
        }
    }

    public class MarkdownSubpage
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public partial class MarkdownViewModel: ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentPageContent))]
        private MarkdownPage _selectedPage;

        [ObservableProperty]
        private TextDocument _currentPageContent;

        [ObservableProperty]
        private bool _isReadOnly = false;

        [ObservableProperty]
        private ObservableCollection<MarkdownPage> _markdownPages;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private ObservableCollection<MarkdownPage> _searchResults;

        public MarkdownViewModel()
        {
            SearchResults = new ObservableCollection<MarkdownPage>();
            CurrentPageContent = new TextDocument();

            MarkdownPages = new ObservableCollection<MarkdownPage>([
                new(){ Id = 1, Title = "Information", Content = "testing" },
            new(){ Id = 2, Title = "Common Issues and Resolutions", Content = "Lorem ipsum"},
            new(){ Id = 3, Title = "FAQs", Content = "Lorem ipsum"}]);
            SelectedPage = MarkdownPages.First(p => p.Id == 1);
            LoadSelectedPageContent();
        }

        private ObservableCollection<MarkdownPage> LoadExamplePages()
        {
            return new ObservableCollection<MarkdownPage>([
                new(){ Id = 1, Title = "Information", Content = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "README.MD")) },
            new(){ Id = 2, Title = "Common Issues and Resolutions", Content = "Lorem ipsum"},
            new(){ Id = 3, Title = "FAQs", Content = "Lorem ipsum"}]);
        }

        partial void OnSelectedPageChanged(MarkdownPage value)
        {
            LoadSelectedPageContent();
        }

        private void LoadSelectedPageContent()
        {
            if (SelectedPage != null)
            {
                //Debug.WriteLine(SelectedPage.Content);
                CurrentPageContent.Text = SelectedPage.Content;
                //Debug.WriteLine(CurrentPageContent.Text);
                IsReadOnly = true;
            }
        }

        [RelayCommand]
        public void NewPage()
        {
            // Preferably you'd create a service to handle this
            var newPage = new MarkdownPage { Id = MarkdownPages.Count + 1, Title = "New Page", Content = "" };
            MarkdownPages.Add(newPage);
            SelectedPage = newPage;
            IsReadOnly = false;
        }

        [RelayCommand]
        private void Edit()
        {
            IsReadOnly = false;
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()
        {
            if (SelectedPage != null)
            {
                SelectedPage.Content = CurrentPageContent.Text;
                IsReadOnly = true;
            }
        }

        private bool CanSave()
        {
            return !IsReadOnly && SelectedPage != null;
        }

        [RelayCommand]
        private void Search()
        {
            // Implement search functionality
        }

        [RelayCommand]
        private void SelectPage(object page)
        {
            if (page is MarkdownPage markdownPage)
            {
                SelectedPage = markdownPage;
            }
            else if (page is MarkdownSubpage markdownSubpage && MarkdownPages.FirstOrDefault(p => p.Subpages.Contains(markdownSubpage)) is MarkdownPage outerPage)
            {
                SelectedPage = outerPage;
            }
        }
    }
}
