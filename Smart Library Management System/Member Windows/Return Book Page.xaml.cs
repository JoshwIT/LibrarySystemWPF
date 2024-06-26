﻿using Microsoft.Win32;
using Smart_Library_Management_System.Member_Windows;
using Smart_Library_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
namespace Smart_Library_Management_System
{
    /// <summary>
    /// Interaction logic for Return_Book_Page.xaml
    /// </summary>
    public partial class Return_Book_Page : Window
    {
        List<BookList> books = new List<BookList>();
        List<UserBorrowedBooksList> bBooks = new List<UserBorrowedBooksList>();

        private string acc_ID = string.Empty;
        SLMSDataContext slms = null;
        public Return_Book_Page()
        {
            InitializeComponent();
            var BooksList = from books in Connections._slms.Books
                            select books.Title;

            
            lbBooks.ItemsSource = BooksList.ToList();
        }
        public Return_Book_Page(string acc_id)
        {
            InitializeComponent();
            acc_ID = acc_id;

            slms = new SLMSDataContext(Properties.Settings.Default.LibWonderConnectionString);

            var bookList = from bookdetails in slms.Books
                           select bookdetails;

            var BookDocs = from books in slms.Book_Documentations
                            where books.Acc_ID == acc_id
                            select books;
            foreach (var b in BookDocs)
            {
                foreach (var bb2 in bookList)
                {
                    //Filters to books only borrowed and not returned
                    if(b.Book_ID == bb2.Book_ID && bb2.Status == "Borrowed")
                    {
                        books.Add(new BookList
                        {
                            bookID = b.Book_ID
                        });
                        break;
                    }
                }
            }

            List<string> bookTitles = new List<string>();

            foreach(var book in books) //loops on each BookList
            {
                
                var userborrowedbooks = from books2 in slms.Books
                                        where books2.Book_ID == book.bookID
                                        select books2.Book_ID;
                foreach(string borrowedBooks in userborrowedbooks)
                {
                    foreach (var bd in bookList)
                    {
                        if (borrowedBooks == bd.Book_ID)
                        {
                            bBooks.Add(new UserBorrowedBooksList
                            {
                                bID = bd.Book_ID,
                                bTitle = bd.Title,
                                bAuthor = bd.Author,
                                bGenre = bd.Genre,
                                bPublishYear = (int)bd.Publish_Year,
                                bStatus = bd.Status,
                                bBook_Image = bd.Book_Image.ToArray(),
                                bQr_Image = bd.QR_Path.ToArray(),
                            });
                            bookTitles.Add(bd.Title);
                            break;

                        }
                    }
                }
            }

            lbBooks.ItemsSource = bookTitles.ToList();

            DisableButtons();
        }
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image files|*.bmp;*.jpg;*.png";
            openDialog.FilterIndex = 1;

            if (openDialog.ShowDialog() == true)
            {
                imagePicture.Source = new BitmapImage(new Uri(openDialog.FileName));
            }
        }
        private void btnTakeAPhoto_Click(object sender, RoutedEventArgs e)
        {
            TakeAPhoto takeAPhoto = new TakeAPhoto();
            takeAPhoto.ShowDialog();

            if (TempImageStorer.image != null)
            {
                BitmapImage bmpImg = ConvertBitmapToBitmapImage(TempImageStorer.image);

                int cropWidth = 320; 
                int cropHeight = 300; 
                int cropX = (bmpImg.PixelWidth - cropWidth) / 2;
                int cropY = (bmpImg.PixelHeight - cropHeight) / 2;

                CroppedBitmap croppedImage = new CroppedBitmap(
                    bmpImg,
                    new Int32Rect(cropX, cropY, cropWidth, cropHeight)
                );
                imagePicture.Source = croppedImage;
            }
        }
        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            TempImageStorer.memStream.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = TempImageStorer.memStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (imagePicture.Source != null)
            {
                DateTime dt = DateTime.Now;
                string book_id = string.Empty;
                var getBooks = from books in Connections._slms.Books
                               select books;

                foreach (var title in getBooks)
                {
                    if (title.Title == tbBookTitle.Text)
                    {
                        book_id = title.Book_ID;
                    }
                }

                byte[] imageData = null;
                BitmapImage bitmapImage = imagePicture.Source as BitmapImage;
                imageData = BitmapSourceToByteArray(bitmapImage);

                Connections._slms.Prod_ReturnBook(book_id, User.Account_ID, imageData, dt);
                Connections._slms.SubmitChanges();
                MessageBox.Show("You returned a book!");

                SLMSDataContext slms = new SLMSDataContext(Properties.Settings.Default.LibWonderConnectionString);
                
                this.Content = null;
                Return_Book_Page rbp = new Return_Book_Page(acc_ID);
                rbp.Show();
                this.Close();
                //var BooksList = from books in slms.Book_Documentations
                //                where books.Acc_ID == acc_ID
                //                select books;

                //lbBooks.ItemsSource = BooksList.ToList();
            }
            else
            {
                MessageBox.Show("There must be a picture of the book to be returned!");
            }
        }
        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            User_Homepage UH = new User_Homepage(acc_ID);
            UH.Show();
            this.Close();
        }
        private void lbBooks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbBooks.SelectedIndex >= 0 && lbBooks.SelectedIndex < lbBooks.Items.Count)
            {
                var selectedBook = lbBooks.SelectedItem.ToString();
                var BookInfo = Connections._slms.Books.FirstOrDefault(o => o.Title == selectedBook);
                if (BookInfo != null)
                {
                    tbBookTitle.Text = BookInfo.Title;
                    tbAuthor.Text = BookInfo.Author;
                    tbGenre.Text = BookInfo.Genre;
                    tbPublishDate.Text = BookInfo.Publish_Year.ToString();
                    tbStatus.Text = BookInfo.Status;
                    imagePicture.Source = null;
                    EnableButtons();
                }
            }
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            slms = new SLMSDataContext(Properties.Settings.Default.LibWonderConnectionString);

            var bookList = from bookdetails in slms.Books
                           select bookdetails;

            List<string> bookTitles = new List<string>();

            foreach (var book in books) //loops on each BookList
            {

                var userborrowedbooks = from books2 in slms.Books
                                        where books2.Book_ID == book.bookID
                                        select books2.Book_ID;
                foreach (string borrowedBooks in userborrowedbooks)
                {
                    foreach (var bd in bookList)
                    {
                        if (borrowedBooks == bd.Book_ID)
                        {
                            bBooks.Add(new UserBorrowedBooksList
                            {
                                bID = bd.Book_ID,
                                bTitle = bd.Title,
                                bAuthor = bd.Author,
                                bGenre = bd.Genre,
                                bPublishYear = (int)bd.Publish_Year,
                                bStatus = bd.Status,
                                bBook_Image = bd.Book_Image.ToArray(),
                                bQr_Image = bd.QR_Path.ToArray(),
                            });
                            bookTitles.Add(bd.Title);
                            break;

                        }
                    }
                }
            }

            lbBooks.ItemsSource = bookTitles.ToList();
            tbBookTitle.Text = string.Empty;
            tbAuthor.Text = string.Empty;
            tbGenre.Text = string.Empty;
            tbPublishDate.Text = string.Empty;
            tbStatus.Text = string.Empty;
            imagePicture.Source = null;
            tbSearchBar.Text = string.Empty;

            DisableButtons();
        }
        private void tbSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            lbBooks.ItemsSource = null;

            string searchBook = tbSearchBar.Text;
            List<string> bookTitles = new List<string>();

            if (string.IsNullOrEmpty(searchBook))
            {
                // If the search bar is empty, show the original list
                foreach (var book in bBooks)
                {
                    bookTitles.Add(book.bTitle);
                }
            }
            else
            {
                // If the search bar is not empty, filter the original list
                var searchResults = bBooks.Where(b => b.bTitle.ToLower().Contains(searchBook.ToLower()));
                foreach (var book in searchResults)
                {
                    bookTitles.Add(book.bTitle);
                }
            }

            lbBooks.ItemsSource = bookTitles;
        }
        private byte[] BitmapSourceToByteArray(BitmapSource bitmapSource)
        {
            byte[] byteArray;
            using (MemoryStream ms = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(ms);
                byteArray = ms.ToArray();
            }
            return byteArray;
        }
        private void DisableButtons()
        {
            btnTakeAPhoto.IsEnabled = false;
            btnUpload.IsEnabled = false;
        }
        private void EnableButtons() 
        { 
            btnTakeAPhoto.IsEnabled = true;
            btnUpload.IsEnabled = true;
        }
    }
}
