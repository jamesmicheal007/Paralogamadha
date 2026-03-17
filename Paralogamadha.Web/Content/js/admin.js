/* ============================================================
   Paralogamadha Basilica — admin.js
   Admin CMS Panel JavaScript: sidebar, DataTables, uploads,
   AJAX actions, confirm dialogs, Quill, Chart.js
   ============================================================ */

(function ($) {
  'use strict';

  /* ── Sidebar toggle ────────────────────────────────────────── */
  const sidebar     = document.getElementById('adminSidebar');
  const mainArea    = document.getElementById('adminMain');
  const toggleBtn   = document.getElementById('sidebarToggle');
  const closeBtn    = document.getElementById('sidebarClose');

  if (toggleBtn && sidebar) {
    toggleBtn.addEventListener('click', () => sidebar.classList.toggle('open'));
  }
  if (closeBtn && sidebar) {
    closeBtn.addEventListener('click', () => sidebar.classList.remove('open'));
  }
  // Close sidebar on outside click (mobile)
  document.addEventListener('click', (e) => {
    if (window.innerWidth < 992 && sidebar &&
        !sidebar.contains(e.target) && e.target !== toggleBtn) {
      sidebar.classList.remove('open');
    }
  });

  /* ── DataTables init ───────────────────────────────────────── */
  if (typeof $.fn.DataTable !== 'undefined') {
    $('table.data-table').DataTable({
      responsive:    true,
      pageLength:    25,
      language: {
        search:        '',
        searchPlaceholder: 'Search…',
        lengthMenu:    'Show _MENU_ entries',
        info:          'Showing _START_–_END_ of _TOTAL_',
        paginate:      { previous: '‹', next: '›' }
      },
      columnDefs: [{ orderable: false, targets: '_all' }],
      order: [],
    });
  }

  /* ── Quill rich-text editors ───────────────────────────────── */
  document.querySelectorAll('.quill-editor').forEach(container => {
    const hiddenInput = document.getElementById(container.dataset.target);
    const quill = new Quill(container, {
      theme: 'snow',
      modules: {
        toolbar: [
          [{ header: [2, 3, false] }],
          ['bold', 'italic', 'underline'],
          [{ list: 'ordered' }, { list: 'bullet' }],
          ['link'],
          ['clean']
        ]
      }
    });
    // Pre-populate
    if (hiddenInput && hiddenInput.value) {
      quill.root.innerHTML = hiddenInput.value;
    }
    // Sync to hidden input on form submit
    const form = container.closest('form');
    if (form && hiddenInput) {
      form.addEventListener('submit', () => {
        hiddenInput.value = quill.root.innerHTML;
      });
    }
  });

  /* ── Image upload preview ──────────────────────────────────── */
  document.querySelectorAll('.img-upload-wrap input[type="file"]').forEach(input => {
    const preview = input.closest('.img-upload-wrap').querySelector('.img-preview');
    input.addEventListener('change', function () {
      const file = this.files[0];
      if (!file || !preview) return;
      const reader = new FileReader();
      reader.onload = (e) => {
        preview.src = e.target.result;
        preview.style.display = 'block';
      };
      reader.readAsDataURL(file);
    });

    // Drag & drop
    const wrap = input.closest('.img-upload-wrap');
    wrap.addEventListener('dragover', (e) => { e.preventDefault(); wrap.classList.add('drag-over'); });
    wrap.addEventListener('dragleave', () => wrap.classList.remove('drag-over'));
    wrap.addEventListener('drop', (e) => {
      e.preventDefault();
      wrap.classList.remove('drag-over');
      if (e.dataTransfer.files.length) {
        input.files = e.dataTransfer.files;
        input.dispatchEvent(new Event('change'));
      }
    });
  });

  /* ── Multi-photo upload (Gallery) ─────────────────────────── */
  const multiUpload = document.getElementById('multiPhotoUpload');
  if (multiUpload) {
    multiUpload.addEventListener('change', async function () {
      const albumId   = document.getElementById('albumIdForUpload')?.value;
      const token     = document.querySelector('[name="__RequestVerificationToken"]')?.value;
      const grid      = document.getElementById('photoUploadGrid');
      const progress  = document.getElementById('uploadProgress');
      const progBar   = document.getElementById('uploadProgressBar');
      if (!albumId || !grid) return;

      const files     = Array.from(this.files);
      let uploaded    = 0;
      if (progress) progress.style.display = 'block';

      for (const file of files) {
        const formData = new FormData();
        formData.append('files', file);
        formData.append('__RequestVerificationToken', token);

        try {
          const res  = await fetch(`/admin/galleryadmin/uploadphotos?albumId=${albumId}`, {
            method: 'POST', body: formData
          });
          const json = await res.json();
          if (json.success && json.data) {
            json.data.forEach(p => {
              const div = document.createElement('div');
              div.className = 'photo-thumb-wrap';
              div.dataset.photoId = p.photoId;
              div.innerHTML = `
                <img src="${p.thumbnailUrl || p.imageUrl}" alt="" loading="lazy" />
                <button class="photo-delete-btn" onclick="deletePhoto(${p.photoId}, this)" type="button">
                  <i class='bx bx-x'></i>
                </button>`;
              grid.appendChild(div);
            });
          }
        } catch (err) {
          console.error('Upload failed for', file.name, err);
        }
        uploaded++;
        if (progBar) progBar.style.width = Math.round((uploaded / files.length) * 100) + '%';
      }
      if (progress) setTimeout(() => progress.style.display = 'none', 1000);
    });
  }

  /* ── Delete photo ──────────────────────────────────────────── */
  window.deletePhoto = function (photoId, btn) {
    adminConfirm('Delete this photo?', 'This cannot be undone.', async () => {
      const token = document.querySelector('[name="__RequestVerificationToken"]')?.value;
      const res   = await fetch(`/admin/galleryadmin/deletephoto`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `id=${photoId}&__RequestVerificationToken=${encodeURIComponent(token)}`
      });
      const json = await res.json();
      if (json.success) {
        btn?.closest('.photo-thumb-wrap')?.remove();
        showAdminToast('success', 'Photo deleted.');
      }
    });
  };

  /* ── AJAX Delete with confirm ──────────────────────────────── */
  document.querySelectorAll('[data-ajax-delete]').forEach(btn => {
    btn.addEventListener('click', (e) => {
      e.preventDefault();
      const url    = btn.dataset.ajaxDelete;
      const label  = btn.dataset.label || 'this item';
      const row    = btn.closest('tr');
      const card   = btn.closest('[data-deletable]');

      adminConfirm(`Delete ${label}?`, 'This action cannot be undone.', async () => {
        const token = document.querySelector('[name="__RequestVerificationToken"]')?.value;
        try {
          const res  = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `__RequestVerificationToken=${encodeURIComponent(token)}`
          });
          const json = await res.json();
          if (json.success) {
            row?.remove();
            card?.remove();
            showAdminToast('success', json.message || 'Deleted successfully.');
          } else {
            showAdminToast('error', json.message || 'Delete failed.');
          }
        } catch {
          showAdminToast('error', 'Network error. Please try again.');
        }
      });
    });
  });

  /* ── AJAX Review (prayer, testimonial, booking) ─────────────── */
  document.querySelectorAll('[data-ajax-review]').forEach(btn => {
    btn.addEventListener('click', async () => {
      const url      = btn.dataset.ajaxReview;
      const statusId = btn.dataset.statusId;
      const entityId = btn.dataset.entityId;
      const notesEl  = document.getElementById('adminNotes_' + entityId);
      const notes    = notesEl ? notesEl.value : '';
      const token    = document.querySelector('[name="__RequestVerificationToken"]')?.value;
      const origText = btn.innerHTML;

      btn.disabled = true;
      btn.innerHTML = '<span class="pbl-spinner pbl-spinner-sm"></span>';

      try {
        const res  = await fetch(url, {
          method: 'POST',
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          body: `id=${entityId}&statusId=${statusId}&adminNotes=${encodeURIComponent(notes)}&__RequestVerificationToken=${encodeURIComponent(token)}`
        });
        const json = await res.json();
        if (json.success) {
          showAdminToast('success', json.message || 'Updated.');
          setTimeout(() => location.reload(), 1200);
        } else {
          showAdminToast('error', json.message || 'Update failed.');
        }
      } catch {
        showAdminToast('error', 'Network error.');
      } finally {
        btn.disabled  = false;
        btn.innerHTML = origText;
      }
    });
  });

  /* ── Toggle switch (featured, active, isLive) ───────────────── */
  document.querySelectorAll('[data-ajax-toggle]').forEach(toggle => {
    toggle.addEventListener('change', async function () {
      const url   = this.dataset.ajaxToggle;
      const field = this.dataset.field;
      const id    = this.dataset.id;
      const val   = this.checked;
      const token = document.querySelector('[name="__RequestVerificationToken"]')?.value;

      try {
        const res  = await fetch(url, {
          method: 'POST',
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          body: `id=${id}&${field}=${val}&__RequestVerificationToken=${encodeURIComponent(token)}`
        });
        const json = await res.json();
        if (!json.success) {
          this.checked = !val; // revert
          showAdminToast('error', 'Update failed.');
        } else {
          showAdminToast('success', 'Updated.');
        }
      } catch {
        this.checked = !val;
        showAdminToast('error', 'Network error.');
      }
    });
  });

  /* ── Dashboard Chart ────────────────────────────────────────── */
  const donationChart = document.getElementById('donationChart');
  if (donationChart && typeof Chart !== 'undefined') {
    const labels = donationChart.dataset.labels ? JSON.parse(donationChart.dataset.labels) : [];
    const values = donationChart.dataset.values ? JSON.parse(donationChart.dataset.values) : [];

    new Chart(donationChart, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label:           'Donations (₹)',
          data:            values,
          backgroundColor: 'rgba(27,42,91,0.7)',
          borderColor:     '#1B2A5B',
          borderWidth:     0,
          borderRadius:    6,
        }]
      },
      options: {
        responsive:          true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { grid: { display: false } },
          y: { grid: { color: '#f0f4f8' }, ticks: { callback: v => '₹' + v.toLocaleString() } }
        }
      }
    });
  }

  /* ── Confirm dialog ────────────────────────────────────────── */
  function adminConfirm(title, message, onConfirm) {
    let modal = document.getElementById('pblConfirmModal');
    if (!modal) {
      modal = document.createElement('div');
      modal.id        = 'pblConfirmModal';
      modal.className = 'modal fade confirm-modal';
      modal.innerHTML = `
        <div class="modal-dialog modal-dialog-centered modal-sm">
          <div class="modal-content">
            <div class="modal-header border-0 pb-0">
              <div class="confirm-icon danger" id="confirmIcon"><i class='bx bx-trash'></i></div>
            </div>
            <div class="modal-body text-center pt-2">
              <h5 id="confirmTitle" class="mb-2"></h5>
              <p id="confirmMsg" class="text-muted small mb-0"></p>
            </div>
            <div class="modal-footer border-0 pt-2 justify-content-center gap-2">
              <button class="btn btn-sm btn-secondary px-4" data-bs-dismiss="modal">Cancel</button>
              <button class="btn btn-sm btn-danger px-4" id="confirmOkBtn">Yes, delete</button>
            </div>
          </div>
        </div>`;
      document.body.appendChild(modal);
    }

    modal.querySelector('#confirmTitle').textContent = title;
    modal.querySelector('#confirmMsg').textContent   = message;

    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();

    const okBtn = modal.querySelector('#confirmOkBtn');
    const newOk = okBtn.cloneNode(true);
    okBtn.replaceWith(newOk);
    newOk.addEventListener('click', () => {
      bsModal.hide();
      onConfirm();
    });
  }

  /* ── Admin toast ────────────────────────────────────────────── */
  window.showAdminToast = function (type, message) {
    const icons = { success: 'bx-check-circle text-success', error: 'bx-x-circle text-danger', warning: 'bx-error text-warning' };
    const toast = document.createElement('div');
    toast.className = `pbl-toast ${type}`;
    toast.style.bottom = '24px';
    toast.innerHTML = `
      <i class="bx ${icons[type] || 'bx-bell'} toast-icon"></i>
      <div class="toast-msg">${message}</div>`;
    document.body.appendChild(toast);
    setTimeout(() => { toast.style.opacity = '0'; toast.style.transition = 'opacity 0.4s'; }, 3200);
    setTimeout(() => toast.remove(), 3700);
  };

  /* ── Sort order drag handle (placeholder, pairs with SortableJS if added) ── */
  // If SortableJS is included: Sortable.create(document.getElementById('sortableList'), { handle: '.drag-handle', onEnd: saveOrder });

  /* ── Auto-dismiss flash alerts ──────────────────────────────── */
  document.querySelectorAll('.alert-autohide').forEach(alert => {
    setTimeout(() => {
      const bsAlert = bootstrap.Alert.getInstance(alert) || new bootstrap.Alert(alert);
      bsAlert.close();
    }, 4000);
  });

})(jQuery);
