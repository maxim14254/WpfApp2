# WpfApp2
### Данное приложение администрирует пользователей.
#### Возможности:
- Просмотр списка пользоватей (username, имя, роли)
- Создание, редактирование, редактирование пользователей
#### Свойства пользователя:
- Логин (задается при создании не изменяется)
- Имя
- Email (уникальный и обязательный)
- Пароль
- Роль (может быть несколько)
Список возможных ролей **"Администратор" "Редактор справочников"**. Редактор справочников не может выполнять никаких действий над пользователями, может только получать список пользователей.
#### Реализация:
- Окно авторизации (WinForms)
- Данные хранятся в БД
- Окно отображения выполенено в WPF с использованием паттерна MVVM 

P.S.  
В папке WpfApp2 находится файл сформированной базы данных Database1.mdf. Подключать ее надо к (LocalDB)\MSSQLLocalDB, но если у вас другой сервер, строку подключения можно изменить в файлах Avtorization.cs (строка 35), MainWindow.xaml.cs (строка 44), User.cs (строки 42 и 47) 
 
