using CommunityToolkit.Maui.Core;

namespace CameraTest
{
    public partial class MainPage : ContentPage
    {


        private bool _isCameraInitialized = false;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Запрашиваем разрешение на камеру
            await CheckAndRequestCameraPermission();

        }

        /// <summary>
        /// Запрос разрешения на камеру у пользователя
        /// </summary>
        private async Task CheckAndRequestCameraPermission()
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Разрешение не получено",
                    "Для работы приложения необходимо разрешение на использование камеры",
                    "OK");
            }
        }

        /// <summary>
        /// Обработчик кнопки "Сделать фото"
        /// </summary>
        private async void OnTakePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                // В версии 3.0.2 используется CaptureImage
                var photoStream = await CameraView.CaptureImage(CancellationToken.None);

                if (photoStream != null)
                {
                    // Отображаем фото
                    PreviewImage.Source = ImageSource.FromStream(() => photoStream);

                    // Сохраняем фото
                    await SavePhotoToFile(photoStream);

                    await DisplayAlert("Успех", "Фото сохранено!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось сделать фото: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Сохранение фото в файл
        /// </summary>
        private async Task SavePhotoToFile(Stream photoStream)
        {
            try
            {
                string fileName = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                string folderPath = Path.Combine(FileSystem.AppDataDirectory, "Photos");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = Path.Combine(folderPath, fileName);

                using (var fileStream = File.Create(filePath))
                {
                    photoStream.Position = 0;
                    await photoStream.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        /// <summary>
        /// Переключение камеры
        /// </summary>
        private async void OnSwitchCameraClicked(object sender, EventArgs e)
        {
            try
            {
                // Получаем доступные камеры напрямую из CameraView
                var availableCameras = await CameraView.GetAvailableCameras(CancellationToken.None);

                if (availableCameras.Count <= 1)
                {
                    await DisplayAlert("Информация", "Только одна камера доступна", "OK");
                    return;
                }

                var currentCamera = CameraView.SelectedCamera;
                var nextCamera = availableCameras.FirstOrDefault(c => c != currentCamera);

                if (nextCamera != null)
                {
                    // Просто меняем выбранную камеру
                    CameraView.SelectedCamera = nextCamera;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось переключить камеру: {ex.Message}", "OK");
            }
        }
    }
}
