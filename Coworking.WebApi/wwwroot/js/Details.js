function openBookingModal(roomId, roomName) {
    $('#roomId').val(roomId);
    $('#modalTitle').text(`Бронирование: ${roomName}`);
    $('#bookingModal').modal('show');
}

async function createBooking() {
    const roomId = $('#roomId').val();
    const startTime = $('#startTime').val();
    const endTime = $('#endTime').val();

    if (!startTime || !endTime) {
        alert('Пожалуйста, выберите время начала и окончания бронирования.');
        return;
    }

    try {
        const response = await fetch('/api/bookings', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                roomId: roomId,
                startTime: new Date(startTime).toISOString(),
                endTime: new Date(endTime).toISOString()
            })
        });

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('bookingModal')).hide();
            // alert("Бронирование успешно создано!");
            alert(`Бронирование комнаты ${roomId} с ${startTime} по ${endTime} успешно создано!`);
        } else {
            alert("Ошибка при создании бронирования");
        }
    } catch (e) {
        console.error(e);
        alert("Произошла ошибка");
    }
    $('#bookingModal').modal('hide');
}