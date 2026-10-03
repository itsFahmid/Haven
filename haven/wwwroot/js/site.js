/**
 * OBHOY Platform - Production Frontend Core Script
 * Child & Adult Safety Education, Mental Health & Emergency Recovery Platform
 * Multi-Language Engine (Bangla / English), Quick Exit Safety, Crisis Escalation & Payment Gateways
 */

(function () {
    'use strict';

    // -------------------------------------------------------------
    // 1. Multi-Language (Bilingual) Engine: Bangla (Default) / English
    // -------------------------------------------------------------
    window.ObhoyLang = window.HavenLang = {
        current: (localStorage.getItem('obhoy_lang') || localStorage.getItem('haven_lang')) || 'bn',

        set: function (lang) {
            if (lang !== 'bn' && lang !== 'en') lang = 'bn';
            this.current = lang;
            localStorage.setItem('obhoy_lang', lang); localStorage.setItem('haven_lang', lang);
            document.cookie = `Obhoy_Lang=${lang};path=/;max-age=31536000;SameSite=Lax`;
            document.documentElement.lang = lang;
            document.body.setAttribute('data-current-lang', lang);

            // Update all DOM elements with bilingual data attributes
            document.querySelectorAll('[data-bn][data-en]').forEach(el => {
                const text = lang === 'bn' ? el.getAttribute('data-bn') : el.getAttribute('data-en');
                if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
                    if (el.getAttribute('placeholder')) {
                        el.setAttribute('placeholder', text);
                    }
                } else {
                    el.innerHTML = text;
                }
            });

            // Update language toggle buttons in UI
            document.querySelectorAll('.lang-toggle-btn').forEach(btn => {
                const targetLang = btn.getAttribute('data-set-lang');
                if (targetLang === lang) {
                    btn.classList.add('bg-teal-700', 'text-white', 'shadow-sm');
                    btn.classList.remove('text-slate-600', 'hover:bg-slate-100');
                } else {
                    btn.classList.remove('bg-teal-700', 'text-white', 'shadow-sm');
                    btn.classList.add('text-slate-600', 'hover:bg-slate-100');
                }
            });

            window.dispatchEvent(new CustomEvent('obhoyLanguageChanged', { detail: { lang: lang } }));
        },

        init: function () {
            this.set(this.current);
        }
    };

    // -------------------------------------------------------------
    // 2. Universal "Quick Exit" Emergency Safety Mechanism
    // -------------------------------------------------------------
    window.obhoyQuickExit = window.havenQuickExit = function () {
        try {
            // Wipe client-side traces instantly
            localStorage.clear();
            sessionStorage.clear();

            // Clear cookies
            document.cookie.split(";").forEach(function (c) {
                document.cookie = c.replace(/^ +/, "").replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/");
            });

            // Cancel any active streaming timers
            if (window.HavenChat && window.ObhoyChat.activeTypingTimers) {
                window.ObhoyChat.activeTypingTimers.forEach(function (t) { clearInterval(t); });
                window.ObhoyChat.activeTypingTimers = [];
            }

            // Explicitly purge active chat stream and inputs
            const chatStream = document.getElementById('chatMessagesStream');
            if (chatStream) chatStream.innerHTML = '';
            const chatInput = document.getElementById('chatMessageInput');
            if (chatInput) chatInput.value = '';

            // Overwrite document DOM immediately to prevent flashing on back-press
            document.body.innerHTML = "<div style='display:flex;justify-content:center;align-items:center;height:100vh;background:#fff;font-family:sans-serif;'>Closing website and opening YouTube...</div>";

            // Open new tab/window to youtube.com
            try {
                window.open("https://www.youtube.com", "_blank");
            } catch (err) {}

            // Hard replace current history and jump to youtube.com
            window.location.replace("https://www.youtube.com");

            // Attempt to close window if permitted by browser
            try {
                window.close();
            } catch (err) {}
        } catch (e) {
            window.location.href = "https://www.youtube.com";
        }
    };

    // Purge in-memory chat session state on browser tab close or navigation away
    const purgeChatState = function () {
        const chatStream = document.getElementById('chatMessagesStream');
        if (chatStream) chatStream.innerHTML = '';
        const chatInput = document.getElementById('chatMessageInput');
        if (chatInput) chatInput.value = '';
    };
    window.addEventListener('pagehide', purgeChatState);
    window.addEventListener('beforeunload', purgeChatState);

    // Listen for ESC key emergency escape
    window.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            const target = e.target;
            if (target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.tagName === 'SELECT' || target.isContentEditable)) {
                return;
            }

            const activeModal = document.querySelector('.obhoy-modal:not(.hidden), .haven-modal:not(.hidden)');
            if (activeModal && !activeModal.classList.contains('urgent-crisis-modal')) {
                // If standard modal open, ESC closes modal first
                closeAllModals();
            } else {
                // Otherwise ESC initiates Quick Exit
                window.obhoyQuickExit();
            }
        }
    });

    // -------------------------------------------------------------
    // 3. Modal Helpers
    // -------------------------------------------------------------
    window.openModal = function (modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.remove('hidden');
            modal.classList.add('flex');
            document.body.style.overflow = 'hidden';
        }
    };

    window.closeModal = function (modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.add('hidden');
            modal.classList.remove('flex');
            document.body.style.overflow = '';
        }
    };

    window.closeAllModals = function () {
        document.querySelectorAll('.obhoy-modal, .haven-modal').forEach(m => {
            m.classList.add('hidden');
            m.classList.remove('flex');
        });
        document.body.style.overflow = '';
    };

    // -------------------------------------------------------------
    // 4. Acute Risk Crisis Escalation Trigger
    // -------------------------------------------------------------
    window.triggerCrisisEscalation = function (keywordMatched) {
        const crisisModal = document.getElementById('crisisEscalationModal');
        if (crisisModal) {
            const keywordBadge = document.getElementById('crisisDetectedTerm');
            if (keywordBadge && keywordMatched) {
                keywordBadge.textContent = keywordMatched;
            }
            openModal('crisisEscalationModal');
            playSafetyChime();
        }
    };

    function playSafetyChime() {
        try {
            const ctx = new (window.AudioContext || window.webkitAudioContext)();
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = 'sine';
            osc.frequency.setValueAtTime(440, ctx.currentTime);
            osc.frequency.exponentialRampToValueAtTime(880, ctx.currentTime + 0.3);
            gain.gain.setValueAtTime(0.08, ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.5);
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start();
            osc.stop(ctx.currentTime + 0.5);
        } catch (e) {
            // Audio context blocked or not supported
        }
    }

    // -------------------------------------------------------------
    // 5. Interactive Anonymous AI Chatbot Engine
    // -------------------------------------------------------------
    window.ObhoyChat = window.HavenChat = {
        signalRConnection: null,
        activeTypingTimers: [],

        highRiskPatterns: [
            /suicide/i, /kill myself/i, /end my life/i, /hang myself/i, /poison/i, /cut myself/i, /die/i,
            /আত্মহত্যা/i, /মরে যাব/i, /মরতে চাই/i, /বাঁচতে চাই না/i, /ফাঁস/i, /বিষ খাব/i, /নিজেকে শেষ/i, /হাত কাটা/i
        ],

        sendMessage: function () {
            const input = document.getElementById('chatMessageInput');
            if (!input) return;
            const text = input.value.trim();
            if (!text) return;

            const lang = (window.HavenLang && window.ObhoyLang.current) || 'bn';
            const timeStr = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

            // 1. Append User Message immediately
            this.appendMessage('user', text, timeStr);
            input.value = '';

            // 2. Client-side high-risk keyword check (instant escalation modal)
            let matchedTerm = null;
            for (let regex of this.highRiskPatterns) {
                if (regex.test(text)) {
                    matchedTerm = text.match(regex)[0];
                    break;
                }
            }
            if (matchedTerm) {
                setTimeout(function () { window.triggerCrisisEscalation(matchedTerm); }, 300);
            }

            // 3. Show typing indicator
            this.showTypingIndicator();

            // 4. Send via SignalR (response arrives via ReceiveBotResponse), else HTTP, else offline
            const self = this;
            if (this.signalRConnection &&
                typeof signalR !== 'undefined' &&
                this.signalRConnection.state === signalR.HubConnectionState.Connected) {
                this.signalRConnection.invoke('SendMessage', 'Anonymous Ally', text, lang)
                    .catch(function (err) {
                        console.warn('SignalR invoke failed, using HTTP fallback:', err);
                        self._fetchHttpResponse(text, lang, matchedTerm);
                    });
            } else {
                self._fetchHttpResponse(text, lang, matchedTerm);
            }
        },

        _fetchHttpResponse: function (text, lang, matchedTerm) {
            const self = this;
            fetch('/Hotline/SendMessage', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest' },
                body: JSON.stringify({ text: text, lang: lang })
            })
            .then(function (r) { return r.ok ? r.json() : Promise.reject(r.status); })
            .then(function (data) {
                self.handleBotResponse({
                    messageEn: data.messageEn,
                    messageBn: data.messageBn,
                    isHighRisk: data.isHighRisk,
                    triggerEscalationModal: data.triggerEscalationModal,
                    crisisHelpline: data.crisisHelpline,
                    timestamp: data.timestamp
                });
            })
            .catch(function () {
                // Offline fallback
                self.hideTypingIndicator();
                const response = self.generateEmpatheticReply(text, matchedTerm);
                const time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                self.streamBotMessage(response.isCrisis ? 'crisis' : 'bot', response.message, time, response.helpline || null);
                if (response.isCrisis && window.triggerCrisisEscalation) {
                    setTimeout(function () { window.triggerCrisisEscalation('Offline Crisis Match'); }, 400);
                }
            });
        },

        handleBotResponse: function (data) {
            this.hideTypingIndicator();
            if (!data) return;
            const lang = (window.HavenLang && window.ObhoyLang.current) || 'bn';
            const msg = lang === 'bn' ? (data.messageBn || data.messageEn) : (data.messageEn || data.messageBn);
            const time = data.timestamp || new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            const type = (data.isHighRisk || data.triggerEscalationModal) ? 'crisis' : 'bot';
            const helpline = (data.isHighRisk || data.triggerEscalationModal) ? (data.crisisHelpline || '1098 / 999') : null;
            this.streamBotMessage(type, msg, time, helpline);
            if (data.triggerEscalationModal && window.triggerCrisisEscalation) {
                setTimeout(function () { window.triggerCrisisEscalation('AI Crisis Signal'); }, 500);
            }
        },

        streamBotMessage: function (type, fullText, time, helpline) {
            const chatStream = document.getElementById('chatMessagesStream');
            if (!chatStream || !fullText) return;

            const msgDiv = document.createElement('div');
            msgDiv.className = 'flex flex-col items-start mb-4 animate-fade-in';

            const bubbleClass = type === 'crisis' ? 'chat-bubble-crisis' : 'chat-bubble-bot';

            let helplineHtml = '';
            if (helpline) {
                helplineHtml =
                    '<div class="mt-3 pt-3 border-t border-rose-200 flex flex-wrap items-center gap-2">' +
                    '<a href="tel:1098" class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-rose-600 hover:bg-rose-700 text-white font-medium text-xs rounded-lg transition shadow-sm">' +
                    '\uD83D\uDCDE Child Helpline 1098</a>' +
                    '<a href="tel:999" class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-slate-900 hover:bg-slate-800 text-white font-medium text-xs rounded-lg transition shadow-sm">' +
                    '\uD83D\uDEA8 National Emergency 999</a>' +
                    '<a href="tel:01779554391" class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-teal-700 hover:bg-teal-800 text-white font-medium text-xs rounded-lg transition shadow-sm">' +
                    '\uD83D\uDC9A Kaan Pete Roi</a>' +
                    '</div>';
            }

            msgDiv.innerHTML =
                '<div class="max-w-[85%] md:max-w-[80%] p-4 ' + bubbleClass + ' text-sm leading-relaxed shadow-sm">' +
                '<span class="bot-text-content"></span>' +
                '<span class="stream-cursor inline-block w-1.5 h-3.5 bg-current ml-0.5 animate-pulse"></span>' +
                '<div class="bot-helpline-container hidden">' + helplineHtml + '</div>' +
                '</div>' +
                '<span class="text-[11px] text-slate-400 mt-1 px-1">' + time + '</span>';

            chatStream.appendChild(msgDiv);
            chatStream.scrollTop = chatStream.scrollHeight;

            const contentSpan = msgDiv.querySelector('.bot-text-content');
            const cursorSpan = msgDiv.querySelector('.stream-cursor');
            const helplineContainer = msgDiv.querySelector('.bot-helpline-container');
            const self = this;
            let index = 0;
            const textLength = fullText.length;
            const step = textLength > 200 ? 4 : 2;

            const timer = setInterval(function () {
                index += step;
                if (index >= textLength) {
                    index = textLength;
                    clearInterval(timer);
                    if (cursorSpan) cursorSpan.remove();
                    if (contentSpan) contentSpan.innerHTML = self.formatMarkdown(fullText);
                    if (helplineContainer && helpline) helplineContainer.classList.remove('hidden');
                } else {
                    if (contentSpan) contentSpan.textContent = fullText.substring(0, index);
                }
                chatStream.scrollTop = chatStream.scrollHeight;
            }, 18);

            this.activeTypingTimers.push(timer);
        },



        appendMessage: function (type, text, time, helpline) {
            const chatStream = document.getElementById('chatMessagesStream');
            if (!chatStream) return;

            const msgDiv = document.createElement('div');
            msgDiv.className = `flex flex-col ${type === 'user' ? 'items-end' : 'items-start'} mb-4 animate-fade-in`;

            let bubbleClass = 'chat-bubble-bot';
            if (type === 'user') bubbleClass = 'chat-bubble-user';
            if (type === 'crisis') bubbleClass = 'chat-bubble-crisis';

            let helplineHtml = '';
            if (helpline) {
                helplineHtml = `
                    <div class="mt-3 pt-3 border-t border-rose-200 flex flex-wrap items-center gap-2">
                        <a href="tel:1098" class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-rose-600 hover:bg-rose-700 text-white font-medium text-xs rounded-lg transition">
                            📞 Child Helpline 1098
                        </a>
                        <a href="tel:999" class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-slate-900 hover:bg-slate-800 text-white font-medium text-xs rounded-lg transition">
                            🚨 National Emergency 999
                        </a>
                        <a href="tel:01779554391" class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-teal-700 hover:bg-teal-800 text-white font-medium text-xs rounded-lg transition">
                            💚 Kaan Pete Roi
                        </a>
                    </div>
                `;
            }

            msgDiv.innerHTML = `
                <div class="max-w-[85%] md:max-w-[75%] p-4 ${bubbleClass} text-sm leading-relaxed shadow-sm">
                    ${this.formatMarkdown(text)}
                    ${helplineHtml}
                </div>
                <span class="text-[11px] text-slate-400 mt-1 px-1">${time}</span>
            `;

            chatStream.appendChild(msgDiv);
            chatStream.scrollTop = chatStream.scrollHeight;
        },

        showTypingIndicator: function () {
            const chatStream = document.getElementById('chatMessagesStream');
            if (!chatStream) return;

            let indicator = document.getElementById('typingIndicator');
            if (!indicator) {
                indicator = document.createElement('div');
                indicator.id = 'typingIndicator';
                indicator.className = 'flex items-center gap-2 text-slate-400 text-xs py-2 px-3 mb-2';
                indicator.innerHTML = `
                    <span class="inline-block w-2 h-2 rounded-full bg-teal-500 animate-pulse"></span>
                    <span class="inline-block w-2 h-2 rounded-full bg-teal-500 animate-pulse" style="animation-delay: 0.2s"></span>
                    <span class="inline-block w-2 h-2 rounded-full bg-teal-500 animate-pulse" style="animation-delay: 0.4s"></span>
                    <span data-bn="অভয় এআই সহমর্মী উত্তর লিখছে..." data-en="OBHOY AI is thinking empathetically...">অভয় এআই লিখছে...</span>
                `;
                chatStream.appendChild(indicator);
            }
            indicator.style.display = 'flex';
            chatStream.scrollTop = chatStream.scrollHeight;
        },

        hideTypingIndicator: function () {
            const indicator = document.getElementById('typingIndicator');
            if (indicator) indicator.style.display = 'none';
        },

        generateEmpatheticReply: function (rawText, isCrisisMatched) {
            const lang = (window.HavenLang && window.ObhoyLang.current) || 'bn';
            const text = rawText.toLowerCase();

            if (isCrisisMatched) {
                return {
                    isCrisis: true,
                    helpline: '1098 / 999 / 01779554391',
                    message: lang === 'bn'
                        ? 'আমি বুঝতে পারছি আপনি এই মুহূর্তে তীব্র কষ্টের মধ্য দিয়ে যাচ্ছেন। একটি কথা সবসময় মনে রাখবেন: **আপনি একা নন, এবং আপনার জীবনের গুরুত্ব অপরিসীম।** অনুগ্রহ করে এখনই নিচের সংকটকালীন নম্বরে কল করুন। আমাদের প্রশিক্ষিত কাউন্সেলররা আপনার পাশে আছেন।'
                        : 'I can hear how overwhelmed and hurt you feel right now. Please know this: **You are not alone, and your life matters deeply.** Please reach out right now to a certified crisis specialist who is ready to listen without judgment.'
                };
            }

            if (text.includes('photo') || text.includes('blackmail') || text.includes('ছবি') || text.includes('হুমকি') || text.includes('ব্ল্যাকমেইল')) {
                return {
                    isCrisis: false,
                    message: lang === 'bn'
                        ? '🛡️ **সাইবার ব্ল্যাকমেইল জরুরি নির্দেশিকা:**\n1. অপরাধীকে কোনো টাকা বা নতুন ছবি পাঠাবেন না।\n2. অপরাধীর আইডি, মেসেজ ও ফোন নম্বরের স্পষ্ট ফুল-স্ক্রিনশট সংরক্ষণ করুন।\n3. চ্যাট ডিলিট করবেন না (এটি আইনি প্রমাণ)।\n4. জরুরি সহায়তার জন্য চাইল্ড হেল্পলাইন **১০৯৮** বা পুলিশ সাইবার সাপোর্ট ফর উইমেন **০১৩২-০০০০৮৮৮** এ যোগাযোগ করুন।'
                        : '🛡️ **Cyber Blackmail Emergency Protocol:**\n1. Do **NOT** pay any money or send further media.\n2. Preserve full-screen screenshots with timestamps and URL links.\n3. Keep the chats intact as legal evidence.\n4. Call Child Helpline **1098**, Police Cyber Support **01320000888**, or National 999.'
                };
            }

            if (text.includes('panic') || text.includes('anxiety') || text.includes('ভয়') || text.includes('প্যানিক') || text.includes('অস্থির')) {
                return {
                    isCrisis: false,
                    message: lang === 'bn'
                        ? '🌿 **চলুন মনকে শান্ত করি:** এই মুহূর্তে আপনি নিরাপদ। **৪-৭-৮ ব্রিদিং পদ্ধতি** চেষ্টা করুন:\n- নাক দিয়ে ৪ সেকেন্ড শ্বাস নিন\n- ৭ সেকেন্ড শ্বাসটি ধরে রাখুন\n- মুখ দিয়ে ৮ সেকেন্ড ধরে ধীরে ধীরে শ্বাস ছাড়ুন।\nআপনার চারপাশের ৫টি পরিচিত জিনিস দেখুন এবং অনুভব করুন।'
                        : '🌿 **Take a Deep Gentle Breath:** You are safe in this moment. Try the **4-7-8 Somatic Grounding**:\n- Inhale through your nose for 4 seconds\n- Hold your breath gently for 7 seconds\n- Exhale smoothly through your mouth for 8 seconds.\nLook around and name 5 colors you can see.'
                };
            }

            return {
                isCrisis: false,
                message: lang === 'bn'
                    ? 'আপনার অনুভূতি শেয়ার করার জন্য ধন্যবাদ। হেভেন আপনার ১০০% বেনামী ও নিরাপদ প্ল্যাটফর্ম। আপনি চাইলে আমাদের **কোর্স সেকশন** থেকে সাইবার সুরক্ষা শিখতে পারেন, অথবা **থেরাপি ডিরেক্টরি** থেকে ভেরিফায়েড প্রফেশনালের সাথে বিনামূল্যে/কম খরচে সেশন বুক করতে পারেন।'
                    : 'Thank you for reaching out. OBHOY is your completely anonymous, safe sanctuary. You can explore our safety courses, practice grounding exercises, or book a confidential session with a verified specialist anytime.'
            };
        },

        formatMarkdown: function (str) {
            if (!str) return '';
            return str
                .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
                .replace(/\*(.*?)\*/g, '<em>$1</em>')
                .replace(/\n/g, '<br/>');
        },

        sendQuickPrompt: function (btn) {
            const lang = (window.HavenLang && window.ObhoyLang.current) || 'bn';
            const text = lang === 'bn' ? btn.getAttribute('data-bn') : btn.getAttribute('data-en');
            const input = document.getElementById('chatMessageInput');
            if (input && text) {
                input.value = text;
                this.sendMessage();
            }
        }
    };

    // -------------------------------------------------------------
    // 6. Community Sanctuary Stewardship & Safe Checkout
    // -------------------------------------------------------------
    window.ObhoyPayment = window.HavenPayment = {
        selectedGateway: 'sslcommerz',
        selectedAmount: 600,

        setGateway: function (gateway) {
            this.selectedGateway = gateway;
            document.querySelectorAll('.gateway-card').forEach(card => {
                card.classList.remove('active', 'bkash', 'nagad', 'rocket');
                if (card.getAttribute('data-gateway') === gateway) {
                    card.classList.add('active', gateway);
                }
            });
        },

        setAmount: function (amount, btn) {
            this.selectedAmount = amount;
            document.querySelectorAll('.amount-preset-btn').forEach(b => {
                b.classList.remove('bg-teal-700', 'text-white', 'border-teal-700');
                b.classList.add('bg-white', 'text-slate-700', 'border-slate-200');
            });

            if (btn) {
                if (btn.classList.contains('amount-preset-btn')) {
                    btn.classList.add('bg-teal-700', 'text-white', 'border-teal-700');
                    btn.classList.remove('bg-white', 'text-slate-700', 'border-slate-200');
                }
                const radio = btn.querySelector ? btn.querySelector('input[type="radio"]') : null;
                if (radio) {
                    radio.checked = true;
                    document.querySelectorAll('input[name="presetAmount"]').forEach(r => {
                        const parentLabel = r.closest('label');
                        if (parentLabel) {
                            parentLabel.classList.remove('border-teal-700', 'bg-teal-50/40');
                            parentLabel.classList.add('border-slate-200', 'bg-white');
                        }
                    });
                    const currentLabel = radio.closest('label');
                    if (currentLabel) {
                        currentLabel.classList.remove('border-slate-200', 'bg-white');
                        currentLabel.classList.add('border-teal-700', 'bg-teal-50/40');
                    }
                }
            }

            const customInput = document.getElementById('customAmountInput');
            if (customInput) customInput.value = amount;
        },

        processPayment: function () {
            const customInput = document.getElementById('customAmountInput');
            const amount = customInput ? parseInt(customInput.value) || this.selectedAmount : this.selectedAmount;
            
            // Check opt-in from either modal or donate page
            const optHallOfFame = document.getElementById('optHallOfFameCheck')?.checked 
                               || document.getElementById('modalOptInHallOfFame')?.checked 
                               || false;
            
            const isAnon = optHallOfFame ? false : (document.getElementById('payAnonymousCheck')?.checked ?? true);
            
            const donorName = document.getElementById('donorDisplayNameInput')?.value 
                           || document.getElementById('modalDisplayName')?.value 
                           || document.getElementById('donorNameInput')?.value 
                           || '';
                           
            const recMessage = document.getElementById('donorRecognitionMessageInput')?.value 
                            || document.getElementById('modalRecognitionMessage')?.value 
                            || '';
                            
            const city = document.getElementById('donorCityInput')?.value || '';
            const phone = document.getElementById('donorPhoneInput')?.value || '';

            if (!amount || amount < 10) {
                const isBn = window.ObhoyLang && window.ObhoyLang.current === 'bn';
                alert(isBn 
                    ? 'অনুগ্রহ করে কমপক্ষে ১০ টাকার পরিমাণ উল্লেখ করুন।' 
                    : 'Please enter an amount of at least ৳10.');
                return;
            }

            const btn = document.getElementById('confirmPaymentBtn');
            const originalHtml = btn ? btn.innerHTML : '';
            if (btn) {
                btn.disabled = true;
                const isBn = window.ObhoyLang && window.ObhoyLang.current === 'bn';
                btn.innerHTML = isBn
                    ? `<span class="inline-block animate-spin mr-2">⏳</span> SSLCommerz গেটওয়েতে সংযোগ হচ্ছে...`
                    : `<span class="inline-block animate-spin mr-2">⏳</span> Connecting to SSLCommerz...`;
            }

            fetch('/Payment/InitiateDonation', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    amountBDT: amount,
                    gateway: 'sslcommerz',
                    donorName: donorName,
                    recognitionMessage: recMessage,
                    city: city,
                    mobileNumber: phone,
                    isAnonymous: isAnon,
                    optIntoHallOfFame: optHallOfFame,
                    purpose: 'Micro-Donation'
                })
            })
            .then(res => res.json())
            .then(data => {
                if (data.success && data.gatewayUrl) {
                    window.location.href = data.gatewayUrl;
                } else {
                    const isBn = window.ObhoyLang && window.ObhoyLang.current === 'bn';
                    alert(data.message || (isBn
                        ? 'পেমেন্ট গেটওয়েতে সংযোগ করা যায়নি।'
                        : 'Failed to connect to payment gateway.'));
                    if (btn) {
                        btn.disabled = false;
                        btn.innerHTML = originalHtml;
                    }
                }
            })
            .catch(err => {
                console.error(err);
                const isBn = window.ObhoyLang && window.ObhoyLang.current === 'bn';
                alert(isBn
                    ? 'নেটওয়ার্ক সংযোগ ত্রুটি। অনুগ্রহ করে পুনরায় চেষ্টা করুন।'
                    : 'Network error. Please try again.');
                if (btn) {
                    btn.disabled = false;
                    btn.innerHTML = originalHtml;
                }
            });
        },

        resetPaymentModal: function () {
            const step1 = document.getElementById('paymentStep1');
            const stepSuccess = document.getElementById('paymentStepSuccess');
            if (step1 && stepSuccess) {
                step1.classList.remove('hidden');
                stepSuccess.classList.add('hidden');
            }
        }
    };

    // -------------------------------------------------------------
    // 7. Interactive Therapy Directory & Booking Modal
    // -------------------------------------------------------------
    window.ObhoyTherapy = window.HavenTherapy = {
        selectedTherapist: null,
        selectedSlotId: null,

        openBookingModal: function (therapistId, nameEn, nameBn, fee, degreeEn, degreeBn) {
            this.selectedTherapist = { id: therapistId, nameEn, nameBn, fee, degreeEn, degreeBn };
            const modal = document.getElementById('therapyBookingModal');
            if (!modal) return;

            const nameEl = document.getElementById('bookingTherapistName');
            if (nameEl) {
                const isBn = window.ObhoyLang.current === 'bn';
                nameEl.innerText = isBn ? nameBn : nameEn;
            }

            const feeEl = document.getElementById('bookingFeeAmount');
            if (feeEl) feeEl.innerText = '৳' + fee;

            openModal('therapyBookingModal');
        },

        selectSlot: function (slotId, el) {
            this.selectedSlotId = slotId;
            document.querySelectorAll('.therapy-slot-btn').forEach(btn => {
                btn.classList.remove('bg-teal-700', 'text-white', 'border-teal-700');
                btn.classList.add('bg-slate-50', 'text-slate-700', 'border-slate-200');
            });
            if (el) {
                el.classList.add('bg-teal-700', 'text-white', 'border-teal-700');
                el.classList.remove('bg-slate-50', 'text-slate-700', 'border-slate-200');
            }
        },

        confirmBooking: function () {
            const submitBtn = document.getElementById('submitBookingBtn');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerText = 'Confirming Slot...';
            }

            setTimeout(() => {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerText = 'Confirm Booking';
                }

                closeModal('therapyBookingModal');
                showToast(
                    window.ObhoyLang.current === 'bn'
                        ? 'আপনার গোপনীয় থেরাপি সেশনটি নিশ্চিত করা হয়েছে! কনফার্মেশন কোড: ' + ('OBH-' + Math.floor(1000 + Math.random() * 9000))
                        : 'Confidential therapy session confirmed! Reference code: ' + ('OBH-' + Math.floor(1000 + Math.random() * 9000))
                );
            }, 1000);
        }
    };

    // -------------------------------------------------------------
    // 8. Toast Notification Utility
    // -------------------------------------------------------------
    window.showToast = function (message) {
        let toast = document.getElementById('obhoyToastNotification');
        if (!toast) {
            toast = document.createElement('div');
            toast.id = 'obhoyToastNotification';
            toast.className = 'fixed bottom-6 right-6 z-50 max-w-md bg-slate-900 text-white px-5 py-3.5 rounded-xl shadow-2xl flex items-center gap-3 transition-all transform translate-y-12 opacity-0 text-sm font-medium';
            document.body.appendChild(toast);
        }

        toast.innerHTML = `<span>🛡️</span> <div>${message}</div>`;
        toast.classList.remove('translate-y-12', 'opacity-0');
        toast.classList.add('translate-y-0', 'opacity-100');

        setTimeout(() => {
            toast.classList.add('translate-y-12', 'opacity-0');
            toast.classList.remove('translate-y-0', 'opacity-100');
        }, 4500);
    };

    // -------------------------------------------------------------
    // 9. Interactive Course Progress Toggle
    // -------------------------------------------------------------
    window.toggleModuleStep = function (checkbox, courseId, stepNumber) {
        const isChecked = checkbox.checked;
        const parentCard = checkbox.closest('.course-card');
        if (parentCard) {
            const allCheckboxes = parentCard.querySelectorAll('.module-step-check');
            const total = allCheckboxes.length;
            const completed = parentCard.querySelectorAll('.module-step-check:checked').length;
            const percent = total > 0 ? Math.round((completed / total) * 100) : 0;

            const progressBar = parentCard.querySelector('.course-progress-fill');
            const percentText = parentCard.querySelector('.course-progress-text');
            if (progressBar) progressBar.style.width = percent + '%';
            if (percentText) {
                percentText.innerText = (window.ObhoyLang.current === 'bn')
                    ? `${completed}/${total} সম্পন্ন (${percent}%)`
                    : `${completed}/${total} Completed (${percent}%)`;
            }
        }
    };

    // -------------------------------------------------------------
    // 10. Document Ready Initialization
    // -------------------------------------------------------------
    document.addEventListener('DOMContentLoaded', function () {
        window.ObhoyLang.init();

        // Bind chat input enter key
        const chatInput = document.getElementById('chatMessageInput');
        if (chatInput) {
            chatInput.addEventListener('keypress', function (e) {
                if (e.key === 'Enter') {
                    window.ObhoyChat.sendMessage();
                }
            });
        }

        // -------------------------------------------------------------
        // Navbar Active Page Visual Indicator & Instant Feedback Engine
        // -------------------------------------------------------------
        const updateNavbarActiveState = function () {
            const currentPath = window.location.pathname.toLowerCase().replace(/\/$/, '') || '/';
            const allNavLinks = document.querySelectorAll('.nav-link-item, .nav-mobile-item');

            allNavLinks.forEach(function (link) {
                const href = (link.getAttribute('href') || '').toLowerCase().replace(/\/$/, '') || '/';
                let isMatch = false;

                if (href === '/' && currentPath === '/') {
                    isMatch = true;
                } else if (href !== '/' && (currentPath === href || currentPath.startsWith(href + '/'))) {
                    isMatch = true;
                }

                if (isMatch) {
                    link.classList.add('active', 'text-teal-900', 'font-bold');
                    link.classList.remove('text-slate-600', 'border-transparent');
                    link.setAttribute('aria-current', 'page');
                }
            });
        };

        // Complete top progress bar animation smoothly on page mount
        const navBar = document.getElementById('navProgressBar');
        if (navBar) {
            navBar.style.width = '100%';
            navBar.style.opacity = '1';
            setTimeout(function () {
                navBar.style.opacity = '0';
                setTimeout(function () {
                    navBar.style.width = '0%';
                }, 300);
            }, 180);
        }

        // Sync on DOM ready and history back/forward restoration
        updateNavbarActiveState();
        window.addEventListener('pageshow', updateNavbarActiveState);

        // Bind instant visual feedback for navbar items on click
        const navLinks = document.querySelectorAll('.nav-link-item, .nav-mobile-item');
        navLinks.forEach(function (link) {
            link.addEventListener('click', function (e) {
                if (e.ctrlKey || e.metaKey || e.shiftKey || e.which === 2) return;
                const href = this.getAttribute('href');
                if (!href || href.startsWith('#') || href.startsWith('javascript:')) return;

                const targetPath = (href.split('?')[0] || '').toLowerCase().replace(/\/$/, '') || '/';

                // Instantly update both desktop and mobile items matching target
                document.querySelectorAll('.nav-link-item, .nav-mobile-item').forEach(function (el) {
                    const elHref = (el.getAttribute('href') || '').split('?')[0].toLowerCase().replace(/\/$/, '') || '/';
                    const isTarget = (elHref === targetPath);

                    if (isTarget) {
                        el.classList.add('active', 'text-teal-900', 'font-bold');
                        el.classList.remove('text-slate-600', 'border-transparent');
                        el.setAttribute('aria-current', 'page');
                    } else {
                        el.classList.remove('active', 'text-teal-900', 'font-bold', 'bg-teal-50', 'bg-teal-50/90', 'border-teal-200/80', 'shadow-2xs');
                        el.classList.add('border-transparent');
                        el.removeAttribute('aria-current');
                    }
                });

                // Trigger snappy loading progress bar
                if (navBar) {
                    navBar.style.transition = 'width 250ms ease-out, opacity 150ms ease';
                    navBar.style.opacity = '1';
                    navBar.style.width = '40%';
                    setTimeout(function () {
                        if (navBar.style.opacity === '1') {
                            navBar.style.width = '85%';
                        }
                    }, 100);
                }
            });
        });

        // -------------------------------------------------------------
        // Notification System UI & Live Polling Engine
        // -------------------------------------------------------------
        window.toggleNotificationDropdown = function () {
            const menu = document.getElementById('notificationDropdownMenu');
            if (!menu) return;
            menu.classList.toggle('hidden');
            if (!menu.classList.contains('hidden')) {
                window.fetchNotifications();
            }
        };

        window.fetchNotifications = function () {
            const badge = document.getElementById('notificationBadge');
            const headerBadge = document.getElementById('notificationHeaderBadge');
            const list = document.getElementById('notificationList');
            if (!badge || !list) return;

            fetch('/Notification/GetNotifications')
                .then(r => {
                    if (r.status === 401) return null;
                    return r.ok ? r.json() : null;
                })
                .then(data => {
                    if (!data || !data.success) return;

                    const unread = data.unreadCount || 0;
                    if (unread > 0) {
                        badge.textContent = unread > 99 ? '99+' : unread;
                        badge.classList.remove('hidden');
                        if (headerBadge) {
                            headerBadge.textContent = `${unread} unread`;
                            headerBadge.classList.remove('hidden');
                        }
                    } else {
                        badge.classList.add('hidden');
                        if (headerBadge) {
                            headerBadge.classList.add('hidden');
                        }
                    }

                    if (!data.notifications || data.notifications.length === 0) {
                        list.innerHTML = `
                            <div class="p-6 text-center text-xs text-slate-400">
                                <div class="w-8 h-8 rounded-full bg-slate-100 text-slate-400 flex items-center justify-center mx-auto mb-2 text-sm">🔔</div>
                                <span>কোনো নতুন বিজ্ঞপ্তি নেই / No notifications</span>
                            </div>`;
                        return;
                    }

                    let html = '';
                    data.notifications.forEach(n => {
                        let iconHtml = '🔔';
                        let borderClass = n.isRead ? 'bg-white' : 'bg-teal-50/40 font-semibold';
                        let dotHtml = n.isRead ? '' : '<span class="w-2 h-2 rounded-full bg-teal-600 shrink-0"></span>';

                        if (n.type === 'BookingAccepted') {
                            iconHtml = '✓';
                        } else if (n.type === 'BookingRejected') {
                            iconHtml = '✕';
                        } else if (n.type === 'BookingPending') {
                            iconHtml = '⏳';
                        }

                        html += `
                            <div class="p-3.5 hover:bg-slate-50 transition flex items-start gap-3 cursor-pointer ${borderClass}" onclick="handleNotificationClick(${n.id}, '${n.linkUrl}')">
                                <div class="w-7 h-7 rounded-xl ${n.type === 'BookingAccepted' ? 'bg-emerald-100 text-emerald-700' : n.type === 'BookingRejected' ? 'bg-rose-100 text-rose-700' : 'bg-teal-100 text-teal-800'} flex items-center justify-center text-xs font-bold shrink-0 mt-0.5">
                                    ${iconHtml}
                                </div>
                                <div class="flex-grow min-w-0">
                                    <div class="flex items-center justify-between gap-1">
                                        <h4 class="text-xs font-bold text-slate-900 truncate">${n.title}</h4>
                                        ${dotHtml}
                                    </div>
                                    <p class="text-[11px] text-slate-600 mt-0.5 leading-tight line-clamp-2">${n.message}</p>
                                    <span class="text-[10px] text-slate-400 mt-1 block">${n.timeAgo}</span>
                                </div>
                            </div>`;
                    });

                    list.innerHTML = html;
                })
                .catch(() => {
                    // Ignore network failure gracefully
                });
        };

        window.handleNotificationClick = function (id, linkUrl) {
            fetch('/Notification/MarkAsRead?id=' + id, { method: 'POST' })
                .finally(() => {
                    if (linkUrl && linkUrl !== '#' && linkUrl.length > 1) {
                        window.location.href = linkUrl;
                    } else {
                        window.fetchNotifications();
                    }
                });
        };

        window.markAllNotificationsAsRead = function () {
            fetch('/Notification/MarkAllAsRead', { method: 'POST' })
                .then(() => {
                    window.fetchNotifications();
                });
        };

        // Close dropdowns on outside click
        document.addEventListener('click', function (e) {
            const notifContainer = document.getElementById('notificationDropdownContainer');
            const notifMenu = document.getElementById('notificationDropdownMenu');
            if (notifContainer && notifMenu && !notifContainer.contains(e.target)) {
                notifMenu.classList.add('hidden');
            }

            const profileMenuContainer = document.getElementById('userMenuContainer');
            const profileMenu = document.getElementById('profileDropdownMenu');
            if (profileMenuContainer && profileMenu && !profileMenuContainer.contains(e.target)) {
                profileMenu.classList.add('hidden');
            }
        });

        // Initialize polling if notification bell exists
        if (document.getElementById('notificationBellBtn')) {
            window.fetchNotifications();
            setInterval(window.fetchNotifications, 20000);
        }
    });

})();

