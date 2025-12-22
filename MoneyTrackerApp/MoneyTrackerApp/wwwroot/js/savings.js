/* Savings Page Scripts */

document.addEventListener('DOMContentLoaded', function () {
    initializeInterestChart();
});

// Icon Selection Logic
function selectIcon(icon, element) {
    const input = document.getElementById('selectedIcon');
    if (input) input.value = icon;

    document.querySelectorAll('.icon-option').forEach(el => el.classList.remove('selected'));
    if (element) element.classList.add('selected');
}

// Color Selection Logic
function selectColor(color, element) {
    const input = document.getElementById('selectedColor');
    if (input) input.value = color;

    document.querySelectorAll('.color-option').forEach(el => el.classList.remove('selected'));
    if (element) element.classList.add('selected');
}

// Render Simple Progress Chart (if needed)
function initializeInterestChart() {
    // Placeholder for future charts
}

// Celebration Effect
function triggerConfetti() {
    if (typeof confetti === 'undefined') return;

    var duration = 3000;
    var end = Date.now() + duration;

    (function frame() {
        confetti({
            particleCount: 5,
            angle: 60,
            spread: 55,
            origin: { x: 0 },
            colors: ['#2563EB', '#10B981', '#F43F5E', '#7C3AED']
        });
        confetti({
            particleCount: 5,
            angle: 120,
            spread: 55,
            origin: { x: 1 },
            colors: ['#2563EB', '#10B981', '#F43F5E', '#7C3AED']
        });

        if (Date.now() < end) {
            requestAnimationFrame(frame);
        }
    }());
}

// Form Validation and Submission Helpers
function validateGoalForm() {
    // Add custom validation if needed
    return true;
}

// Modal Actions
function openDeleteModal(id) {
    document.getElementById('deleteGoalId').value = id;
    var modal = new bootstrap.Modal(document.getElementById('deleteGoalModal'));
    modal.show();
}

function openEditModal(id, name, target, current, date, icon, color) {
    document.getElementById('editGoalId').value = id;
    document.getElementById('editGoalName').value = name;
    document.getElementById('editTargetAmount').value = target;
    document.getElementById('editCurrentAmount').value = current;
    document.getElementById('editTargetDate').value = date;

    // Icon
    document.getElementById('editSelectedIcon').value = icon;
    document.querySelectorAll('#editIconContainer .icon-option').forEach(el => {
        el.classList.remove('selected');
        if (el.dataset.icon === icon) el.classList.add('selected');
    });

    // Color
    document.getElementById('editSelectedColor').value = color;
    document.querySelectorAll('#editColorContainer .color-option').forEach(el => {
        el.classList.remove('selected');
        if (el.dataset.color === color) el.classList.add('selected');
    });

    var modal = new bootstrap.Modal(document.getElementById('editGoalModal'));
    modal.show();
}

function selectEditIcon(icon, element) {
    document.getElementById('editSelectedIcon').value = icon;
    document.querySelectorAll('#editIconContainer .icon-option').forEach(el => el.classList.remove('selected'));
    if (element) element.classList.add('selected');
}

function selectEditColor(color, element) {
    document.getElementById('editSelectedColor').value = color;
    document.querySelectorAll('#editColorContainer .color-option').forEach(el => el.classList.remove('selected'));
    if (element) element.classList.add('selected');
}
