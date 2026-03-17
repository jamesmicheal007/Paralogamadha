/* ============================================================
   Paralogamadha Basilica — app.js
   Public-facing JavaScript: navbar, hero, tabs, forms, toast
   ============================================================ */

(function () {
  'use strict';

  /* ── Navbar scroll effect ──────────────────────────────────── */
  const navbar = document.getElementById('siteNavbar');
  if (navbar) {
    const onScroll = () => {
      navbar.classList.toggle('scrolled', window.scrollY > 60);
    };
    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
  }

  /* ── Back to top ───────────────────────────────────────────── */
  const btt = document.querySelector('.back-to-top');
  if (btt) {
    window.addEventListener('scroll', () => {
      btt.classList.toggle('visible', window.scrollY > 400);
    }, { passive: true });
    btt.addEventListener('click', () => window.scrollTo({ top: 0, behavior: 'smooth' }));
  }

  /* ── Language switcher dropdown ────────────────────────────── */
  const langBtn  = document.querySelector('.lang-btn');
  const langDrop = document.querySelector('.lang-dropdown');
  if (langBtn && langDrop) {
    langBtn.addEventListener('click', (e) => {
      e.stopPropagation();
      langDrop.classList.toggle('open');
    });
    document.addEventListener('click', () => langDrop.classList.remove('open'));
  }

  /* ── Hero Swiper ────────────────────────────────────────────── */
  if (document.querySelector('.hero-swiper')) {
    new Swiper('.hero-swiper', {
      loop:           true,
      speed:          900,
      autoplay:       { delay: 5500, disableOnInteraction: false },
      effect:         'fade',
      fadeEffect:     { crossFade: true },
      pagination:     { el: '.swiper-pagination', clickable: true },
      navigation:     { nextEl: '.swiper-button-next', prevEl: '.swiper-button-prev' },
    });
  }

  /* ── Testimonial Swiper ─────────────────────────────────────── */
  if (document.querySelector('.testimonial-swiper')) {
    new Swiper('.testimonial-swiper', {
      loop:        true,
      speed:       700,
      autoplay:    { delay: 5000 },
      slidesPerView: 1,
      spaceBetween: 24,
      breakpoints: {
        768:  { slidesPerView: 2 },
        1200: { slidesPerView: 3 },
      },
      pagination:  { el: '.swiper-pagination', clickable: true },
    });
  }

  /* ── GLightbox ──────────────────────────────────────────────── */
  if (typeof GLightbox !== 'undefined') {
    GLightbox({ selector: '.glightbox', touchNavigation: true, loop: true });
  }

  /* ── Stat counters ──────────────────────────────────────────── */
  const counters = document.querySelectorAll('[data-count]');
  if (counters.length && typeof CountUp !== 'undefined') {
    const io = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (!entry.isIntersecting) return;
        const el  = entry.target;
        const end = parseInt(el.dataset.count, 10);
        const cu  = new CountUp.CountUp(el, end, { duration: 2.5, separator: ',' });
        if (!cu.error) cu.start();
        io.unobserve(el);
      });
    }, { threshold: 0.5 });
    counters.forEach(c => io.observe(c));
  }

  /* ── Mass timing tabs (day selector) ───────────────────────── */
  const dayBtns    = document.querySelectorAll('.day-btn');
  const massPanels = document.querySelectorAll('.mass-panel');
  if (dayBtns.length) {
    dayBtns.forEach(btn => {
      btn.addEventListener('click', () => {
        dayBtns.forEach(b => b.classList.remove('active'));
        massPanels.forEach(p => p.classList.add('d-none'));
        btn.classList.add('active');
        const target = document.getElementById('mass-day-' + btn.dataset.day);
        if (target) target.classList.remove('d-none');
      });
    });
    // Activate today
    const todayBtn = document.querySelector('.day-btn.today');
    if (todayBtn) todayBtn.click();
    else if (dayBtns[0]) dayBtns[0].click();
  }

  /* ── Prayer category selector ───────────────────────────────── */
  const catBtns   = document.querySelectorAll('.prayer-category-btn');
  const catInput  = document.getElementById('PrayerCategory');
  if (catBtns.length && catInput) {
    catBtns.forEach(btn => {
      btn.addEventListener('click', () => {
        catBtns.forEach(b => b.classList.remove('selected'));
        btn.classList.add('selected');
        catInput.value = btn.dataset.category;
      });
    });
  }

  /* ── Prayer request form ────────────────────────────────────── */
  const prayerForm = document.getElementById('prayerForm');
  if (prayerForm) {
    prayerForm.addEventListener('submit', async (e) => {
      e.preventDefault();
      if (!validateForm(prayerForm)) return;

      const btn = prayerForm.querySelector('button[type="submit"]');
      setLoading(btn, true);

      const data = new URLSearchParams(new FormData(prayerForm));
      try {
        const res  = await fetch(prayerForm.action, {
          method: 'POST', body: data,
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        });
        const json = await res.json();
        if (json.success) {
          prayerForm.style.display = 'none';
          document.getElementById('prayerSuccess').style.display = 'block';
        } else {
          showToast('error', 'Error', json.message || 'Something went wrong.');
        }
      } catch {
        showToast('error', 'Network Error', 'Please try again.');
      } finally {
        setLoading(btn, false);
      }
    });
  }

  /* ── Testimonial form ───────────────────────────────────────── */
  const testForm = document.getElementById('testimonialForm');
  if (testForm) {
    testForm.addEventListener('submit', async (e) => {
      e.preventDefault();
      const btn  = testForm.querySelector('button[type="submit"]');
      setLoading(btn, true);
      const data = new URLSearchParams(new FormData(testForm));
      try {
        const res  = await fetch(testForm.action, { method: 'POST', body: data,
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' } });
        const json = await res.json();
        if (json.success) {
          showToast('success', 'Thank You!', json.message);
          testForm.reset();
          document.querySelectorAll('.star-radio').forEach(r => r.checked = false);
          renderStars(0);
        } else {
          showToast('error', 'Error', json.message);
        }
      } catch { showToast('error', 'Network Error', 'Please try again.'); }
      finally   { setLoading(btn, false); }
    });
  }

  /* ── Star rating widget ─────────────────────────────────────── */
  const starContainer = document.querySelector('.star-rating-widget');
  if (starContainer) {
    const stars     = starContainer.querySelectorAll('.star-btn');
    const ratingInput = document.getElementById('Rating');
    stars.forEach(star => {
      star.addEventListener('click', () => {
        const val = parseInt(star.dataset.value);
        if (ratingInput) ratingInput.value = val;
        renderStars(val);
      });
      star.addEventListener('mouseenter', () => renderStars(parseInt(star.dataset.value), true));
      starContainer.addEventListener('mouseleave', () => {
        const cur = parseInt(ratingInput?.value || 0);
        renderStars(cur);
      });
    });
  }
  function renderStars(val, hover = false) {
    document.querySelectorAll('.star-btn').forEach(s => {
      const v = parseInt(s.dataset.value);
      s.classList.toggle('filled', v <= val);
      s.classList.toggle('hover',  hover && v <= val);
    });
  }

  /* ── Room booking form ──────────────────────────────────────── */
  const roomForm = document.getElementById('roomBookingForm');
  if (roomForm) {
    // Room selection
    document.querySelectorAll('.room-card[data-room]').forEach(card => {
      card.addEventListener('click', () => {
        document.querySelectorAll('.room-card').forEach(c => c.classList.remove('selected'));
        card.classList.add('selected');
        const hiddenRoomId = document.getElementById('RoomId');
        if (hiddenRoomId) hiddenRoomId.value = card.dataset.room;
      });
    });

    // Availability check
    const startInput = document.getElementById('StartDateTime');
    const endInput   = document.getElementById('EndDateTime');
    const availMsg   = document.getElementById('availabilityMsg');

    async function checkAvailability() {
      const roomId = document.getElementById('RoomId')?.value;
      const start  = startInput?.value;
      const end    = endInput?.value;
      if (!roomId || !start || !end || !availMsg) return;

      availMsg.innerHTML = '<span class="pbl-spinner pbl-spinner-sm"></span> Checking…';
      try {
        const url = `/roombooking/getavailability?roomId=${roomId}&start=${encodeURIComponent(start)}&end=${encodeURIComponent(end)}`;
        const res  = await fetch(url);
        const json = await res.json();
        availMsg.innerHTML = json.data.available
          ? '<span class="text-success"><i class="bx bx-check-circle me-1"></i>Available</span>'
          : '<span class="text-danger"><i class="bx bx-x-circle me-1"></i>Not available for this time</span>';
      } catch { availMsg.innerHTML = ''; }
    }

    if (startInput) startInput.addEventListener('change', checkAvailability);
    if (endInput)   endInput.addEventListener('change', checkAvailability);

    roomForm.addEventListener('submit', async (e) => {
      e.preventDefault();
      if (!validateForm(roomForm)) return;
      const btn  = roomForm.querySelector('button[type="submit"]');
      setLoading(btn, true);
      const data = new URLSearchParams(new FormData(roomForm));
      try {
        const res  = await fetch(roomForm.action, { method: 'POST', body: data,
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' } });
        const json = await res.json();
        if (json.success) {
          document.getElementById('bookingSuccessRef').textContent = json.data.bookingRef;
          roomForm.style.display = 'none';
          document.getElementById('bookingSuccess').style.display = 'block';
        } else {
          showToast('error', 'Booking Failed', json.message);
        }
      } catch { showToast('error', 'Network Error', 'Please try again.'); }
      finally   { setLoading(btn, false); }
    });
  }

  /* ── Donation step wizard ───────────────────────────────────── */
  const donationSteps = document.querySelectorAll('.donation-step');
  if (donationSteps.length) {
    let currentStep = 0;

    // Amount preset buttons
    document.querySelectorAll('.amount-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        document.querySelectorAll('.amount-btn').forEach(b => b.classList.remove('selected'));
        btn.classList.add('selected');
        const amountInput = document.getElementById('donationAmount');
        if (!btn.classList.contains('custom')) {
          amountInput.value = btn.dataset.amount;
          amountInput.readOnly = true;
        } else {
          amountInput.value = '';
          amountInput.readOnly = false;
          amountInput.focus();
        }
      });
    });

    window.nextDonationStep = function () {
      if (currentStep >= donationSteps.length - 1) return;
      if (!validateStep(currentStep)) return;
      donationSteps[currentStep].classList.remove('active');
      currentStep++;
      donationSteps[currentStep].classList.add('active');
      updateStepIndicator();
    };
    window.prevDonationStep = function () {
      if (currentStep <= 0) return;
      donationSteps[currentStep].classList.remove('active');
      currentStep--;
      donationSteps[currentStep].classList.add('active');
      updateStepIndicator();
    };

    function updateStepIndicator() {
      document.querySelectorAll('.step-dot').forEach((dot, i) => {
        dot.classList.toggle('active', i === currentStep);
        dot.classList.toggle('done',   i < currentStep);
      });
      document.querySelectorAll('.step-line').forEach((line, i) => {
        line.classList.toggle('done', i < currentStep);
      });
    }

    function validateStep(step) {
      const inputs = donationSteps[step].querySelectorAll('[required]');
      let valid = true;
      inputs.forEach(inp => {
        if (!inp.value.trim()) {
          inp.classList.add('is-error');
          valid = false;
          inp.addEventListener('input', () => inp.classList.remove('is-error'), { once: true });
        }
      });
      return valid;
    }
  }

  /* ── Gallery filter ─────────────────────────────────────────── */
  const filterBtns = document.querySelectorAll('.filter-btn[data-filter]');
  const albumCards = document.querySelectorAll('.album-card[data-category]');
  if (filterBtns.length) {
    filterBtns.forEach(btn => {
      btn.addEventListener('click', () => {
        filterBtns.forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        const filter = btn.dataset.filter;
        albumCards.forEach(card => {
          const match = filter === 'all' || card.dataset.category === filter;
          card.style.display = match ? '' : 'none';
        });
      });
    });
  }

  /* ── Utility: Form validation ───────────────────────────────── */
  function validateForm(form) {
    let valid = true;
    form.querySelectorAll('[required]').forEach(field => {
      const ok = field.value.trim() !== '';
      field.classList.toggle('is-error', !ok);
      if (!ok) valid = false;
      field.addEventListener('input', () => field.classList.remove('is-error'), { once: true });
    });
    return valid;
  }

  /* ── Utility: Button loading state ─────────────────────────── */
  function setLoading(btn, loading) {
    if (!btn) return;
    if (loading) {
      btn.dataset.originalText = btn.innerHTML;
      btn.innerHTML = '<span class="pbl-spinner pbl-spinner-sm"></span>';
      btn.disabled  = true;
    } else {
      btn.innerHTML = btn.dataset.originalText || btn.innerHTML;
      btn.disabled  = false;
    }
  }

  /* ── Utility: Toast notifications ──────────────────────────── */
  window.showToast = function (type, title, message, duration = 4000) {
    const icons = { success: 'bx-check-circle', error: 'bx-x-circle', warning: 'bx-error' };
    const toast = document.createElement('div');
    toast.className = `pbl-toast ${type}`;
    toast.innerHTML = `
      <i class="bx ${icons[type] || 'bx-bell'} toast-icon text-${type === 'error' ? 'danger' : type === 'warning' ? 'warning' : 'success'}"></i>
      <div>
        <div class="toast-title">${title}</div>
        ${message ? `<div class="toast-msg">${message}</div>` : ''}
      </div>
    `;
    document.body.appendChild(toast);
    setTimeout(() => toast.style.opacity = '0', duration - 400);
    setTimeout(() => toast.remove(), duration);
  };

  /* ── AOS init (reinit if needed) ────────────────────────────── */
  if (typeof AOS !== 'undefined') {
    AOS.init({ duration: 700, once: true, offset: 60 });
  }

})();
