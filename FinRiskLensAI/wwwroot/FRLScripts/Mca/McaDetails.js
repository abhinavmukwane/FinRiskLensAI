// ============================================================================
// Corporate Affairs (MCA) — Corporate Intelligence Report page behavior.
// All data is server-rendered by McaController; this script handles tabs,
// the charge accordion, and the director DIN popup, whose data is fetched
// ON CLICK from /Mca/GetDinDetail (DINResponce in m_StaticResponces).
// ============================================================================

(function () {
    'use strict';

    // ── Tabs ─────────────────────────────────────────────────────────────
    document.querySelectorAll('.mca2-tab').forEach(function (tab) {
        tab.addEventListener('click', function () {
            document.querySelectorAll('.mca2-tab').forEach(t => t.classList.remove('active'));
            document.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));
            tab.classList.add('active');
            var panel = document.getElementById(tab.dataset.tab);
            if (panel) panel.classList.add('active');
        });
    });

    // ── Charge accordion (one open at a time) ────────────────────────────
    document.querySelectorAll('.mca2-accordion-header').forEach(function (header) {
        header.addEventListener('click', function () {
            var body = header.nextElementSibling;
            var isOpen = body && body.classList.contains('open');
            document.querySelectorAll('.mca2-accordion-body').forEach(b => b.classList.remove('open'));
            document.querySelectorAll('.mca2-accordion-header').forEach(h => h.classList.remove('open'));
            if (!isOpen && body) {
                body.classList.add('open');
                header.classList.add('open');
            }
        });
    });

    // ── Director DIN popup ───────────────────────────────────────────────
    var overlay = document.getElementById('dinModalOverlay');
    var modal = document.getElementById('dinModal');
    var cache = {}; // din -> profile, so each DIN is fetched once

    function text(id, val) {
        var el = document.getElementById(id);
        if (el) el.textContent = (val === null || val === undefined || val === '') ? '—' : val;
    }

    function openModal() {
        overlay.classList.add('active');
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeModal() {
        overlay.classList.remove('active');
        modal.classList.remove('active');
        document.body.style.overflow = '';
    }

    function renderProfile(d, fallbackName) {
        var name = (d && d.fullName && d.fullName !== '-') ? d.fullName : (fallbackName || '—');
        text('dinName', name);
        text('dinFullName', name);

        var avatar = document.getElementById('dinAvatar');
        if (avatar) avatar.textContent = (d && d.initials && d.initials !== '?') ? d.initials
            : name.split(' ').filter(Boolean).map(w => w[0]).slice(0, 2).join('').toUpperCase() || '?';

        var din = d ? d.din : '—';
        text('dinNumber', 'DIN: ' + din);
        text('dinDinValue', din);
        text('dinFatherName', d && d.fatherName);
        text('dinDob', d && d.dob);
        text('dinNationality', d && d.nationality);
        text('dinEmail', d && d.email);
        text('dinPresentAddress', d && d.presentAddress);
        text('dinPermanentAddress', d && d.permanentAddress);
        text('dinAppointedOn', d && d.appointedOn);
        text('dinDirectorSince', d && d.directorSince);
        text('dinCompanies', d && d.companies);

        var pan = document.getElementById('dinPan');
        if (pan) {
            pan.textContent = (d && d.pan) || 'Not Available';
            pan.style.color = (d && d.pan && d.pan !== 'Not Available') ? '' : 'var(--danger-red)';
        }

        // Verified pill only when the profile came from a real DIN response.
        var pill = document.getElementById('dinVerifiedPill');
        if (pill) {
            pill.innerHTML = (d && d.dinVerified)
                ? '<i class="bi bi-patch-check-fill me-1"></i>DIN Verified'
                : '<i class="bi bi-clock-history me-1"></i>From MCA Record';
        }
    }

    async function showDirector(din, name) {
        openModal();

        var loading = document.getElementById('dinLoading');
        var content = document.getElementById('dinContent');

        if (cache[din]) {
            renderProfile(cache[din], name);
            return;
        }

        renderProfile(null, name);
        if (loading) loading.style.display = 'block';
        if (content) content.style.display = 'none';

        try {
            // DIN data is fetched only when the user clicks the director.
            var res = await fetch('/Mca/GetDinDetail?din=' + encodeURIComponent(din));
            var json = await res.json();
            cache[din] = (json && json.data) || null;
            renderProfile(cache[din], name);
        } catch (e) {
            renderProfile(null, name);
        }

        if (loading) loading.style.display = 'none';
        if (content) content.style.display = 'block';
    }

    document.querySelectorAll('.director-strip-card').forEach(function (card) {
        card.addEventListener('click', function () {
            showDirector(card.dataset.din, card.dataset.name);
        });
    });

    if (overlay) overlay.addEventListener('click', closeModal);
    var closeBtn = document.getElementById('dinModalClose');
    if (closeBtn) closeBtn.addEventListener('click', closeModal);
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeModal();
    });

    // ── Refresh — re-render from the server ──────────────────────────────
    var refresh = document.getElementById('btnRefreshData');
    if (refresh) refresh.addEventListener('click', function () { window.location.reload(); });
})();
