# Преобразователь сырых логов MODBUS в читабельный текст

Первый более-менее серьезный личный проект. За основу взял WPF.

Увлекался темой умного дома, стало интересно про протокол modbus. В процессе изучения проблемой стали логи, которые он делал автоматически - они были крайне нечитабельны.

Поэтому решил написать приложение, которое может преобразовать сырой лог в нормальный, читабельный, информативный текст. 

В планах добавить сериализацию, возможность конвертации в XML и JSON.

Пример сырого лога находится по адресу modbus-logs-parser/raw_log_example.log

![изображение](https://user-images.githubusercontent.com/77939047/114263252-1c896f80-99ed-11eb-8b06-bf1c4824791e.png)

![изображение](https://user-images.githubusercontent.com/77939047/114263339-8dc92280-99ed-11eb-8607-4f97964db62a.png)

В программе предусмотрено сохранение в файл:
![изображение](https://user-images.githubusercontent.com/77939047/114263357-a76a6a00-99ed-11eb-9bb5-a0bb2f4e50a4.png)
