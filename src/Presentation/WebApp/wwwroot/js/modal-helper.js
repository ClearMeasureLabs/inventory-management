// Modal helper functions for Blazor interop
window.modalHelper = {
    show: function (modalId) {
        const modalElement = document.getElementById(modalId);
        if (modalElement) {
            const modal = new bootstrap.Modal(modalElement);
            modal.show();
        }
    },
    hide: function (modalId) {
        const modalElement = document.getElementById(modalId);
        if (modalElement) {
            const modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                modal.hide();
            } else {
                // If no instance exists, try to create one and hide
                const newModal = new bootstrap.Modal(modalElement);
                newModal.hide();
            }
        }
    }
};
