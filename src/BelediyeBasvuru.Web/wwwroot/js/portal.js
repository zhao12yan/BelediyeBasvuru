/* Süleyman Aydoğdu — #20 vatandaş portalı ön yüz uygulaması. */
(() => {
    'use strict';
    document.documentElement.classList.add('portal-js');

    document.querySelectorAll('[data-giris]').forEach(container => {
        const tabs = [...container.querySelectorAll('[data-giris-turu]')];
        function selectTab(tab) {
            tabs.forEach(button => {
                const active = button === tab;
                button.classList.toggle('active', active);
                button.setAttribute('aria-selected', String(active));
                button.tabIndex = active ? 0 : -1;
            });
            container.querySelectorAll('[data-giris-alan]').forEach(panel => {
                panel.hidden = panel.dataset.girisAlan !== tab.dataset.girisTuru;
                panel.querySelectorAll('input, select, button, fieldset').forEach(input => { input.disabled = panel.hidden; });
            });
        }
        tabs.forEach((tab, index) => {
            tab.addEventListener('click', () => selectTab(tab));
            tab.addEventListener('keydown', event => {
                if (!['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key)) return;
                event.preventDefault();
                const next = event.key === 'Home' ? 0 : event.key === 'End' ? tabs.length - 1 : (index + 1) % tabs.length;
                selectTab(tabs[next]);
                tabs[next].focus();
            });
        });
    });

    const form = document.getElementById('basvuruFormu');
    if (form) {
        const panels = [...form.querySelectorAll('[data-adim]')];
        const indicators = [...document.querySelectorAll('[data-adim-gosterge]')];
        const fileInput = form.querySelector('#Fotograflar');
        const list = document.getElementById('fotografListesi');
        const fileError = document.getElementById('fotografHatasi');
        let currentStep = Number(form.dataset.baslangicAdim) || 0;
        let files = [];
        let previewUrls = [];
        let submitting = false;

        function showStep(step, focus = true) {
            currentStep = step;
            panels.forEach((panel, index) => { panel.hidden = index !== step; });
            indicators.forEach((indicator, index) => {
                indicator.classList.toggle('active', index === step);
                indicator.classList.toggle('complete', index < step);
                if (index === step) indicator.setAttribute('aria-current', 'step');
                else indicator.removeAttribute('aria-current');
            });
            document.body.classList.toggle('portal-adim-ilerledi', step > 0);
            if (step === 1) document.getElementById('atananBirim').textContent = form.elements.Kategori.value.split(' (')[0];
            if (step === 3) updateSummary();
            if (focus) {
                panels[step].querySelector('input:not([type="hidden"]), select, textarea, button')?.focus({ preventScroll: true });
                form.closest('.portal-kart').scrollIntoView({ block: 'start' });
            }
        }

        function validStep(step) {
            const inputs = [...panels[step].querySelectorAll('input, select, textarea')];
            for (const input of inputs) {
                if (input.type === 'file') continue;
                let message = '';
                if (input.required && (input.type === 'checkbox' ? !input.checked : !input.value.trim())) message = 'Lütfen bu alanı doldurunuz.';
                else if (input.minLength > 0 && input.value.trim().length < input.minLength) message = `Lütfen en az ${input.minLength} karakter giriniz.`;
                input.setCustomValidity(message);
                if (!input.checkValidity()) { input.reportValidity(); return false; }
            }
            return true;
        }

        function updateSummary() {
            const values = {
                Kategori: form.elements.Kategori.value,
                Adres: [form.elements.Mahalle.value.replace('Batman Merkez - ', ''), form.elements.Adres.value].filter(Boolean).join(' / '),
                Baslik: form.elements.Baslik.value,
                Aciklama: form.elements.Aciklama.value
            };
            form.querySelectorAll('[data-ozet]').forEach(output => { output.textContent = values[output.dataset.ozet] || '—'; });
            document.getElementById('ozetFotograf').textContent = `${files.length} Adet Yüklendi`;
        }

        function renderFiles() {
            previewUrls.forEach(url => URL.revokeObjectURL(url));
            previewUrls = [];
            list.replaceChildren();
            const transfer = new DataTransfer();
            files.forEach(file => transfer.items.add(file));
            fileInput.files = transfer.files;
            document.getElementById('fotografSayisi').textContent = String(files.length);
            if (!files.length) {
                const empty = document.createElement('p');
                empty.className = 'portal-bos-fotograf';
                empty.textContent = 'Henüz fotoğraf eklenmedi. (Opsiyonel)';
                list.append(empty);
            }
            files.forEach((file, index) => {
                const figure = document.createElement('figure');
                figure.className = 'portal-fotograf';
                const img = document.createElement('img');
                const url = URL.createObjectURL(file);
                previewUrls.push(url);
                img.src = url;
                img.alt = file.name;
                const caption = document.createElement('figcaption');
                const name = document.createElement('span');
                name.textContent = file.name;
                const remove = document.createElement('button');
                remove.type = 'button';
                remove.textContent = '×';
                remove.setAttribute('aria-label', `${file.name} fotoğrafını kaldır`);
                remove.addEventListener('click', () => { files.splice(index, 1); renderFiles(); });
                caption.append(name, remove);
                figure.append(img, caption);
                list.append(figure);
            });
        }

        function addFiles(selected) {
            fileError.textContent = '';
            const candidates = [...files];
            for (const file of selected) {
                if (candidates.some(existing => existing.name === file.name && existing.size === file.size && existing.lastModified === file.lastModified)) continue;
                if (candidates.length >= 3) { fileError.textContent = 'En fazla 3 fotoğraf seçebilirsiniz.'; break; }
                if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || !/\.(jpe?g|png|webp)$/i.test(file.name) || file.size <= 0 || file.size > 5 * 1024 * 1024) {
                    fileError.textContent = 'Fotoğraflar JPG, PNG veya WebP olmalı ve dosya başına 5 MB’ı aşmamalıdır.';
                    continue;
                }
                candidates.push(file);
            }
            files = candidates;
            renderFiles();
        }

        fileInput.addEventListener('change', () => addFiles([...fileInput.files]));
        const dropArea = document.getElementById('dosyaBirakmaAlani');
        ['dragenter', 'dragover'].forEach(name => dropArea.addEventListener(name, event => { event.preventDefault(); dropArea.classList.add('surukleniyor'); }));
        ['dragleave', 'drop'].forEach(name => dropArea.addEventListener(name, event => { event.preventDefault(); dropArea.classList.remove('surukleniyor'); }));
        dropArea.addEventListener('drop', event => addFiles([...event.dataTransfer.files]));
        dropArea.addEventListener('click', event => { if (event.target.closest('label, input')) return; fileInput.click(); });
        window.addEventListener('pagehide', () => previewUrls.forEach(url => URL.revokeObjectURL(url)));

        form.querySelectorAll('[data-ileri]').forEach(button => button.addEventListener('click', () => { if (validStep(currentStep)) showStep(currentStep + 1); }));
        form.querySelectorAll('[data-geri]').forEach(button => button.addEventListener('click', () => showStep(currentStep - 1)));
        ['Baslik', 'Aciklama'].forEach(name => {
            const input = form.elements[name];
            const counter = document.getElementById(name === 'Baslik' ? 'baslikSayaci' : 'aciklamaSayaci');
            const updateCounter = () => { counter.textContent = `${input.value.length}/${input.maxLength}`; };
            input.addEventListener('input', updateCounter);
            updateCounter();
        });
        form.addEventListener('input', event => event.target.setCustomValidity?.(''));
        form.addEventListener('change', event => event.target.setCustomValidity?.(''));
        form.addEventListener('submit', event => {
            if (submitting) { event.preventDefault(); return; }
            for (let step = 0; step < panels.length; step++) {
                showStep(step, false);
                if (!validStep(step)) { event.preventDefault(); showStep(step); return; }
            }
            showStep(3, false);
            submitting = true;
            const submit = form.querySelector('[type="submit"]');
            submit.disabled = true;
            submit.textContent = 'Gönderiliyor...';
        });
        window.addEventListener('pageshow', () => {
            if (!submitting) return;
            submitting = false;
            const submit = form.querySelector('[type="submit"]');
            submit.disabled = false;
            submit.textContent = 'Başvuruyu Tamamla ve Gönder →';
        });
        showStep(currentStep, false);
    }

    document.querySelectorAll('[data-detay]').forEach(link => link.addEventListener('click', async event => {
        event.preventDefault();
        const error = document.getElementById('detayHatasi');
        error.textContent = '';
        try {
            const response = await fetch(link.href, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!response.ok) throw new Error('Başvuru ayrıntısı yüklenemedi. Lütfen tekrar deneyiniz.');
            const container = document.getElementById('detayIcerik');
            // İçerik yalnız aynı uygulamanın Razor tarafından HTML kodlanmış kısmi görünümünden gelir.
            container.innerHTML = await response.text();
            const updateForm = container.querySelector('[data-durum-formu]');
            updateForm?.addEventListener('submit', async submitEvent => {
                submitEvent.preventDefault();
                const submitButton = updateForm.querySelector('[type="submit"]');
                if (submitButton.disabled) return;
                submitButton.disabled = true;
                const message = updateForm.querySelector('[data-durum-hatasi]');
                message.textContent = '';
                try {
                    const result = await fetch(updateForm.action, { method: 'POST', body: new FormData(updateForm), headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                    if (!result.ok) throw new Error('Durum güncellenemedi. Oturumunuzu, işlem notunu ve seçtiğiniz durumu kontrol ediniz.');
                    const data = await result.json();
                    window.location.assign(data.url);
                } catch (failure) { message.textContent = failure.message; submitButton.disabled = false; }
            });
            bootstrap.Modal.getOrCreateInstance(document.getElementById('detayModal')).show();
        } catch (failure) { error.textContent = failure.message; }
    }));
})();
