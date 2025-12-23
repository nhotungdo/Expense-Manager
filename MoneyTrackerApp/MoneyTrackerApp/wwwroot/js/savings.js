/* Savings Page Scripts - Modern Redesign */

document.addEventListener('DOMContentLoaded', function () {
    // Initialize any tooltips or popovers if needed
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })
});

// Icon Selection Logic
function selectIcon(icon, element) {
    const input = document.getElementById('selectedIcon');
    if (input) input.value = icon;

    // Remove 'active' from all choices in this container
    const container = element.parentElement;
    container.querySelectorAll('.icon-choice').forEach(el => el.classList.remove('active'));

    // Add 'active' to clicked element
    if (element) element.classList.add('active');
}

// Color Selection Logic
function selectColor(color, element) {
    const input = document.getElementById('selectedColor');
    if (input) input.value = color;

    const container = element.parentElement;
    container.querySelectorAll('.color-choice').forEach(el => el.classList.remove('active'));

    if (element) element.classList.add('active');
}

// Edit Modal Functions
function openEditModal(id, name, target, current, date, icon, color) {
    document.getElementById('editGoalId').value = id;
    document.getElementById('editGoalName').value = name;
    document.getElementById('editTargetAmount').value = target;
    document.getElementById('editCurrentAmount').value = current;

    // Handle date formatting
    if (date) {
        // Ensure date is in YYYY-MM-DD format for input type="date"
        try {
            const d = new Date(date);
            if (!isNaN(d.getTime())) {
                document.getElementById('editTargetDate').value = date.split('T')[0]; // Simple split if ISO
            } else {
                document.getElementById('editTargetDate').value = date;
            }
        } catch (e) {
            document.getElementById('editTargetDate').value = date;
        }
    } else {
        document.getElementById('editTargetDate').value = '';
    }

    // Icon Selection Re-hydration
    document.getElementById('editSelectedIcon').value = icon;
    document.querySelectorAll('#editIconContainer .icon-choice').forEach(el => {
        el.classList.remove('active');
        if (el.dataset.icon === icon) el.classList.add('active');
    });

    // Color Selection Re-hydration
    document.getElementById('editSelectedColor').value = color;
    document.querySelectorAll('#editColorContainer .color-choice').forEach(el => {
        el.classList.remove('active');
        if (el.dataset.color === color) el.classList.add('active');
    });

    // Show Modal
    var modal = new bootstrap.Modal(document.getElementById('editGoalModal'));
    modal.show();
}

// Edit Mode Selectors
function selectEditIcon(icon, element) {
    document.getElementById('editSelectedIcon').value = icon;
    document.querySelectorAll('#editIconContainer .icon-choice').forEach(el => el.classList.remove('active'));
    if (element) element.classList.add('active');
}

function selectEditColor(color, element) {
    document.getElementById('editSelectedColor').value = color;
    document.querySelectorAll('#editColorContainer .color-choice').forEach(el => el.classList.remove('active'));
    if (element) element.classList.add('active');
}

// Delete Modal
function openDeleteModal(id) {
    document.getElementById('deleteGoalId').value = id;
    var modal = new bootstrap.Modal(document.getElementById('deleteGoalModal'));
    modal.show();
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
            colors: ['#6366f1', '#10b981', '#ef4444', '#f59e0b']
        });
        confetti({
            particleCount: 5,
            angle: 120,
            spread: 55,
            origin: { x: 1 },
            colors: ['#6366f1', '#10b981', '#ef4444', '#f59e0b']
        });

        if (Date.now() < end) {
            requestAnimationFrame(frame);
        }
    }());
}
