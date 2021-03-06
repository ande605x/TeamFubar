﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using FællesSpisning.Model;
using Windows.UI.Popups;
using System.Collections.ObjectModel;
using Windows.Storage;
using Newtonsoft.Json;

namespace FællesSpisning.ViewModel
{
    class AdminViewModel : INotifyPropertyChanged
    {

        //Konstanter til filnavne
        const String FileNameJob = "saveJobListe.json";
        const String FileNameMenu = "saveMenuListe.json";
        const String FileNameLock = "saveLåsDic.json";

        //ComboBox options
        public List<string> JobPersonCBoxOptions { get; set; }
        public List<DateTime> ListeMedDateTimes { get; set; }

        public JobPerson Job { get; set; }
        public Menu Menu { get; set; }
        public int SelectedIndex { get; set; }

        public RelayCommand AddJobPersonCommand { get; set; }
        public RelayCommand RemoveJobPersonCommand { get; set; }
        public RelayCommand AddMenuCommand { get; set; }
        public RelayCommand RemoveMenuCommand { get; set; }
        public RelayCommand NyLåsCommand { get; set; }
        public RelayCommand RemoveLåsCommand { get; set; }
        public RelayCommand RydAltCommand { get; set; }


        private PlanlægningSingleton _planSingleton;
        public PlanlægningSingleton PlanSingleton
        {
            get { return _planSingleton; }
            set { _planSingleton = value;
                OnPropertyChanged(nameof(PlanSingleton));
            }
        }

        private JobPerson _selectedJob;
        public JobPerson SelectedJob
        {
            get { return _selectedJob; }
            set
            {
                _selectedJob = value;
                OnPropertyChanged(nameof(SelectedJob));
            }
        }

        private Menu _selectedMenu;
        public Menu SelectedMenu
        {
            get { return _selectedMenu; }
            set
            {
                _selectedMenu = value;
                OnPropertyChanged(nameof(SelectedMenu));
            }
        }

        private String _outPutToUser;
        public String OutPutToUser
        {
            get { return _outPutToUser; }
            set { _outPutToUser = value;
                OnPropertyChanged(nameof(OutPutToUser));
            }
        }

        public AdminViewModel()
        {

            PlanSingleton = PlanlægningSingleton.Instance;

            AddJobPersonCommand = new RelayCommand(AddNewJobPerson, null);
            RemoveJobPersonCommand = new RelayCommand(RemoveSelectedJobPerson, null);
            AddMenuCommand = new RelayCommand(AddNewMenu, null);
            RemoveMenuCommand = new RelayCommand(RemoveSelectedMenu, null);
            NyLåsCommand = new RelayCommand(AddNyLås, null);
            RemoveLåsCommand = new RelayCommand(RemoveLås, null);
            RydAltCommand = new RelayCommand(RydAlt, null);

            SelectedJob = new JobPerson();
            SelectedMenu = new Menu();

            Job = new JobPerson();
            Menu = new Menu();

            AddCBoxOptions();

        }

        public void AddNyLås()
        {
            if (PlanSingleton.LockedDatesDic.ContainsKey(PlanSingleton.SingletonDateTime))
            {
                MessageDialog locked = new MessageDialog("Dato er allerede låst");
                locked.Commands.Add(new UICommand { Label = "Ok" });
                locked.ShowAsync().AsTask();
                OutPutToUser = "";
            } else
            {
                bool BoolLock = true;
                PlanSingleton.AddNewLock(PlanSingleton.SingletonDateTime, BoolLock);
                OutPutToUser = $"{PlanSingleton.SingletonDateTime.ToString("MM/dd")} er låst for tilmeldinger!";
                SaveJsonLockDates_Async();
            }
        }

        public void RemoveLås()
        {
            if(PlanSingleton.LockedDatesDic.Count > 0)
            {
                foreach (DateTime lockObj in PlanSingleton.LockedDatesDic.Keys.ToList())
                {
                    if (lockObj == PlanSingleton.SingletonDateTime)
                    {
                        PlanSingleton.RemoveLock();
                        SaveJsonLockDates_Async();
                        OutPutToUser = $"{PlanSingleton.SingletonDateTime.ToString("MM/dd")} er blevet låst op!";
                    }                
                }
            }
            else
            {
                MessageDialog locked = new MessageDialog("Denne dato er ikke låst!");
                locked.Commands.Add(new UICommand { Label = "Ok" });
                locked.ShowAsync().AsTask();
                OutPutToUser = "";
            }
        }

        public void AddNewJobPerson()
        {
            if(!String.IsNullOrEmpty(Job.JobPersonNavn) && !String.IsNullOrWhiteSpace(Job.JobPersonNavn)) {
            JobPerson tempJob = new JobPerson();

            tempJob.JobDateTime = PlanSingleton.SingletonDateTime;
            tempJob.JobPersonNavn = Job.JobPersonNavn;
            tempJob.JobPersonOpgave = JobPersonCBoxOptions[SelectedIndex];

            PlanSingleton.AddJobPerson(tempJob);
            SaveJobList_Async();
            OutPutToUser = $"{tempJob.JobPersonNavn} er tildelt {tempJob.JobPersonOpgave}!";
            }
            else
            {
                MessageDialog locked = new MessageDialog("Du skal skrive et navn!");
                locked.Commands.Add(new UICommand { Label = "Ok" });
                locked.ShowAsync().AsTask();
                OutPutToUser = "";
            }
        }


        public void AddNewMenu()
        {
            if (!String.IsNullOrEmpty(Menu.MenuMeal) && !String.IsNullOrWhiteSpace(Menu.MenuMeal)) {
            Menu tempMenu = new Menu();

            tempMenu.MenuDateTime = PlanSingleton.SingletonDateTime;
            tempMenu.MenuMeal = Menu.MenuMeal;

            PlanSingleton.AddMenu(tempMenu);
            SaveMenuList_Async();
            OutPutToUser = $"{tempMenu.MenuMeal} er dagens måltid!";
            } else
            {
                MessageDialog locked = new MessageDialog("Du skal skrive en menu!");
                locked.Commands.Add(new UICommand { Label = "Ok" });
                locked.ShowAsync().AsTask();
                OutPutToUser = "";
            }
        }

        public void RemoveSelectedJobPerson()
        {
            if ((SelectedJob != null) && (SelectedJob.JobPersonNavn != null))
            {
                OutPutToUser = $"{SelectedJob.JobPersonNavn} er fjernet som {SelectedJob.JobPersonOpgave}";
                PlanSingleton.RemoveJobPerson(SelectedJob);
                SaveJobList_Async();
            }
            else
            {
                MessageDialog noEvent = new MessageDialog("Vælg person for at slette!");
                noEvent.Commands.Add(new UICommand { Label = "Ok" });
                noEvent.ShowAsync().AsTask();
                OutPutToUser = "";
            }

        }


        public void RemoveSelectedMenu()
        {
            if ((SelectedMenu != null) && (SelectedMenu.MenuMeal != null))
            {
                OutPutToUser = $"{SelectedMenu.MenuMeal} er fjernet!";
                PlanSingleton.RemoveMenu(SelectedMenu);
                SaveMenuList_Async();
            }
            else
            {
                MessageDialog noEvent = new MessageDialog("Vælg menu for at slette!");
                noEvent.Commands.Add(new UICommand { Label = "Ok" });
                noEvent.ShowAsync().AsTask();
                OutPutToUser = "";
            }

        }

        public void RydAlt()
        {
            if(PlanSingleton.MenuListe.Count > 0 || PlanSingleton.JobListe.Count > 0)
            {
                PlanSingleton.MenuListe.Clear();
                PlanSingleton.DisplayMenuOnDateTime();
                PlanSingleton.JobListe.Clear();
                PlanSingleton.DisplayJobOnDateTime();
                SaveJobList_Async();
                SaveMenuList_Async();
                OutPutToUser = "Listerne er blevet ryddet!";
            }
            else
            {
                MessageDialog noEvent = new MessageDialog("Begge lister er tomme!");
                noEvent.Commands.Add(new UICommand { Label = "Ok" });
                noEvent.ShowAsync().AsTask();
                OutPutToUser = "";
            }
        }
        //Add muligheder til ComboBox
        public void AddCBoxOptions()
        {
            JobPersonCBoxOptions = new List<string>() { "Chefkok", "Kok", "Oprydder" };
        }
        

        //Json Save
        public async void SaveJobList_Async()
        {
            StorageFile LocalFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(FileNameJob, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(LocalFile, PlanSingleton.SaveJsonDataJob());

        }

        public async void SaveMenuList_Async()
        {
            StorageFile LocalFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(FileNameMenu, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(LocalFile, PlanSingleton.SaveJsonDataMenu());
        }

        public async void SaveJsonLockDates_Async()
        {
            StorageFile LocalFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(FileNameLock, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(LocalFile, PlanSingleton.SaveJsonLockDates());
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
            
        }

    }
}