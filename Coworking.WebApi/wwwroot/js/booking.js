// === Глобальные переменные состояния ===
let bookingState = {
    roomId: null,
    roomName: '',
    capacity: 0,
    pricePerHour: 0,
    selectedDate: null,
    selectedTime: null,
    duration: null
};

let currentMonth = new Date().getMonth();
let currentYear = new Date().getFullYear();

// === Открытие модального окна ===
function openBookingModal(roomId, roomName, capacity, pricePerHour) {
    // Сброс состояния
    bookingState = {
        roomId,
        roomName,
        capacity,
        pricePerHour,
        selectedDate: null,
        selectedTime: null,
        duration: null
    };

    // Заполнение заголовка
    document.getElementById('modalTitle').textContent = roomName;
    document.getElementById('modalCapacity').textContent = `Вместимость: ${capacity} чел.`;
    document.getElementById('modalPrice').textContent = `${pricePerHour} ₸/час`;

    // Сброс интерфейса
    document.getElementById('timeSection').style.display = 'none';
    document.getElementById('durSection').style.display = 'none';
    document.getElementById('summarySection').style.display = 'none';
    document.getElementById('confirmBtn').style.display = 'none';
    document.getElementById('successMsg').style.display = 'none';

    // Инициализация календаря
    renderCalendar();

    // Показать модалку
    const modal = new bootstrap.Modal(document.getElementById('bookingModal'));
    modal.show();
}

// === Рендер календаря ===
function renderCalendar() {
    const monthNames = ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
        'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'];
    document.getElementById('monthLabel').textContent = `${monthNames[currentMonth]} ${currentYear}`;

    const firstDay = new Date(currentYear, currentMonth, 1);
    const lastDay = new Date(currentYear, currentMonth + 1, 0);
    const startingDay = (firstDay.getDay() + 6) % 7; // Пн=0, Вс=6
    const totalDays = lastDay.getDate();

    const calDays = document.getElementById('calDays');
    calDays.innerHTML = '';

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // Пустые ячейки до первого дня месяца
    for (let i = 0; i < startingDay; i++) {
        calDays.appendChild(document.createElement('div'));
    }

    // Дни месяца
    for (let day = 1; day <= totalDays; day++) {
        const btn = document.createElement('button');
        btn.className = 'cal-day';
        btn.textContent = day;

        const cellDate = new Date(currentYear, currentMonth, day);
        cellDate.setHours(0, 0, 0, 0);

        // Блокируем прошедшие даты
        if (cellDate < today) {
            btn.disabled = true;
        } else {
            btn.onclick = () => selectDate(cellDate, btn);
        }

        // Подсветка сегодня
        if (cellDate.getTime() === today.getTime()) {
            btn.classList.add('today');
        }

        // Подсветка выбранной даты
        if (bookingState.selectedDate && cellDate.getTime() === bookingState.selectedDate.getTime()) {
            btn.classList.add('selected');
        }

        calDays.appendChild(btn);
    }
}

function prevMonth() {
    currentMonth--;
    if (currentMonth < 0) {
        currentMonth = 11;
        currentYear--;
    }
    renderCalendar();
}

function nextMonth() {
    currentMonth++;
    if (currentMonth > 11) {
        currentMonth = 0;
        currentYear++;
    }
    renderCalendar();
}

// === Выбор даты ===
function selectDate(date, btn) {
    bookingState.selectedDate = date;
    bookingState.selectedTime = null;
    bookingState.duration = null;

    // Визуальное выделение
    document.querySelectorAll('.cal-day').forEach(el => el.classList.remove('selected'));
    btn.classList.add('selected');

    // Показать выбор времени
    document.getElementById('timeSection').style.display = 'block';
    document.getElementById('durSection').style.display = 'none';
    document.getElementById('summarySection').style.display = 'none';
    document.getElementById('confirmBtn').style.display = 'none';

    renderTimeSlots();
}

// === Рендер слотов времени ===
function renderTimeSlots() {
    const timeSlots = document.getElementById('timeSlots');
    timeSlots.innerHTML = '';

    // Генерация слотов с 9:00 до 21:00
    for (let hour = 9; hour <= 21; hour++) {
        const btn = document.createElement('button');
        btn.className = 'time-btn';
        const timeStr = `${hour.toString().padStart(2, '0')}:00`;
        btn.textContent = timeStr;

        // Проверка доступности (здесь можно добавить AJAX-запрос к бэкенду)
        // Для демо: блокируем случайные слоты
        const isBlocked = Math.random() < 0.15;
        if (isBlocked) {
            btn.disabled = true;
            btn.title = 'Занято';
        } else {
            btn.onclick = () => selectTime(timeStr, btn);
        }

        // Подсветка выбранного
        if (bookingState.selectedTime === timeStr) {
            btn.classList.add('selected');
        }

        timeSlots.appendChild(btn);
    }
}

function selectTime(timeStr, btn) {
    bookingState.selectedTime = timeStr;

    document.querySelectorAll('.time-btn').forEach(el => el.classList.remove('selected'));
    btn.classList.add('selected');

    // Показать выбор длительности
    document.getElementById('durSection').style.display = 'block';
    document.getElementById('summarySection').style.display = 'none';
    document.getElementById('confirmBtn').style.display = 'none';

    // Сброс длительности
    document.querySelectorAll('.dur-btn').forEach(el => el.classList.remove('selected'));
}

// === Выбор длительности ===
function setDur(hours) {
    bookingState.duration = hours;

    document.querySelectorAll('.dur-btn').forEach(el => el.classList.remove('selected'));
    document.querySelector(`.dur-btn[data-d="${hours}"]`).classList.add('selected');

    updateSummary();
}

// === Обновление итоговой информации ===
function updateSummary() {
    if (!bookingState.selectedDate || !bookingState.selectedTime || !bookingState.duration) return;

    const [hours, minutes] = bookingState.selectedTime.split(':');
    const startDateTime = new Date(bookingState.selectedDate);
    startDateTime.setHours(parseInt(hours), parseInt(minutes));

    const endDateTime = new Date(startDateTime);
    endDateTime.setHours(endDateTime.getHours() + bookingState.duration);

    const total = bookingState.pricePerHour * bookingState.duration;

    const ruOptions = {day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit'};
    document.getElementById('sumDatetime').textContent =
        `${startDateTime.toLocaleDateString('ru-RU', ruOptions)} - ${endDateTime.toLocaleTimeString('ru-RU', {
            hour: '2-digit',
            minute: '2-digit'
        })}`;

    document.getElementById('sumDur').textContent = `${bookingState.duration} ч.`;
    document.getElementById('sumPrice').textContent = `${total} ₸`;

    document.getElementById('summarySection').style.display = 'block';
    document.getElementById('confirmBtn').style.display = 'block';
}

// === Создание бронирования ===
async function createBooking() {
    const btn = document.getElementById('confirmBtn');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> Обработка...';

    const [hours, minutes] = bookingState.selectedTime.split(':');
    const startDateTime = new Date(bookingState.selectedDate);
    startDateTime.setHours(parseInt(hours), parseInt(minutes));

    const endDateTime = new Date(startDateTime);
    endDateTime.setHours(endDateTime.getHours() + bookingState.duration);

    const payload = {
        roomId: bookingState.roomId,
        startTime: startDateTime.toISOString(),
        endTime: endDateTime.toISOString()
    };

    try {
        const response = await fetch('?handler=Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify(payload)
        });

        const result = await response.json();

        if (result.success) {
            // Показать успех
            document.getElementById('confirmBtn').style.display = 'none';
            document.getElementById('summarySection').style.display = 'none';
            document.getElementById('successMsg').style.display = 'block';
            document.getElementById('successDetail').textContent =
                `${bookingState.roomName}, ${bookingState.duration}ч. • ${startDateTime.toLocaleDateString('ru-RU')} ${startDateTime.toLocaleTimeString('ru-RU', {
                    hour: '2-digit',
                    minute: '2-digit'
                })}`;

            // Закрыть модалку через 3 секунды
            setTimeout(() => {
                bootstrap.Modal.getInstance(document.getElementById('bookingModal')).hide();
            }, 3000);
        } else {
            alert('Ошибка: ' + (result.message || 'Не удалось создать бронирование'));
            btn.disabled = false;
            btn.textContent = 'Подтвердить бронирование';
        }
    } catch (error) {
        console.error('Booking error:', error);
        alert('Произошла ошибка при подключении к серверу');
        btn.disabled = false;
        btn.textContent = 'Подтвердить бронирование';
    }
}

// === Инициализация при загрузке ===
document.addEventListener('DOMContentLoaded', function () {
    // Закрытие модального окна - сброс состояния
    document.getElementById('bookingModal').addEventListener('hidden.bs.modal', function () {
        currentMonth = new Date().getMonth();
        currentYear = new Date().getFullYear();
    });
});