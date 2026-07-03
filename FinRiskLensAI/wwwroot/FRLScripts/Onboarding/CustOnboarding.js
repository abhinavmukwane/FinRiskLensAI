
        $(function () {

            // ---------- HELPERS ----------
            function showToast(msg) {
                $('#toastMsg').text(msg);
                new bootstrap.Toast(document.getElementById('liveToast'), { delay: 2600 }).show();
            }

            // ---------- LOGIN MODE DETECTION ----------
            const urlParams = new URLSearchParams(window.location.search);
            const isLoginMode = urlParams.get('mode') === 'login';

            if (isLoginMode) {
                $('body').addClass('login-mode');

                // Adjust Step 1 UI text
                $('#step1 .eyebrow').text('Secure Access');
                $('#step1 .stage-title').text('Login to your MSME Credit Dashboard');
                $('#step1 label[for="udyamInput"]').text('Registered Udyam Number or Mobile');
                $('#udyamInput').attr('placeholder', 'UDYAM-XX-00-0000000 or 10-digit Mobile');
                $('#step1 .help-text').html('<i class="bi bi-info-circle-fill"></i> Enter the Udyam registration ID or mobile number associated with your profile.');
                $('#fetchBtnLabel').html('<i class="bi bi-arrow-right-circle me-1"></i> Send OTP');

                // Adjust Step 3 UI text
                $('#step3 .eyebrow').text('Authentication');
                $('#step3 .stage-title').text('Enter Verification Code');
                $('#step3 .stage-desc').html('Enter the 6-digit OTP sent to your registered contact.');

                // Adjust Step 4 UI text
                $('#step4 .eyebrow').text('Access Granted');
                $('#step4 .stage-title').text('Login Successful 🎉');
                $('#step4 .stage-desc').text('Welcome back! Your identity has been authenticated.');
                $('#step4 .stage-body h3').text('Welcome Back!');
                $('#step4 .stage-body .text-muted').text('Your secure session has been initialized. Click below to load your credit risk intelligence dashboard.');
                $('#btnGoToDashboard').html('<i class="bi bi-speedometer2 me-1"></i> Load Dashboard');
            }

            // 4-step rail: fill percentages at 0%, 33%, 66%, 100%
            var fillMap = { 1: 0, 2: 33, 3: 66, 4: 100 };

            function goToRailStep(n) {
                if (isLoginMode) return; // Rail hidden in login mode
                $('.rail-step').each(function () {
                    var s = parseInt($(this).data('step'));
                    $(this).removeClass('active done');
                    if (s < n) {
                        $(this).addClass('done').find('.node').html('<i class="bi bi-check-lg"></i>');
                    } else if (s === n) {
                        $(this).addClass('active');
                    }
                });
                $('#railFill').css('width', fillMap[n] + '%');
            }

            // Store original icons
            $('.rail-step .node').each(function () { $(this).data('orig', $(this).html()); });

            // ---------- CREDENTIAL MASK ----------
            $('#udyamInput').on('input', function () {
                let rawVal = this.value;
                if (isLoginMode && /^[0-9]/.test(rawVal)) {
                    // Mobile input mask
                    let clean = rawVal.replace(/[^0-9]/g, '').slice(0, 10);
                    this.value = clean;
                } else {
                    // Udyam input mask
                    let v = rawVal.toUpperCase().replace(/[^A-Z0-9]/g, '');
                    let out = 'UDYAM';
                    if (v.startsWith('UDYAM')) v = v.slice(5);
                    if (v.length > 0) out += '-' + v.slice(0, 2);
                    if (v.length > 2) out += '-' + v.slice(2, 4);
                    if (v.length > 4) out += '-' + v.slice(4, 11);
                    this.value = out;
                }
            });

            // ---------- STEP 1 → LOADER → STEP 2 / STEP 3 ----------
            $('#btnFetch').on('click', function () {
                const val = $('#udyamInput').val().trim();

                if (isLoginMode) {
                    const isUdyam = /^UDYAM-[A-Z]{2}-\d{2}-\d{7}$/.test(val);
                    const isMobile = /^\d{10}$/.test(val);

                    if (!isUdyam && !isMobile) {
                        $('#udyamError').removeClass('d-none').find('span').text('Please enter a valid Udyam number (UDYAM-XX-00-0000000) or a 10-digit mobile number.');
                        return;
                    }
                    $('#udyamError').addClass('d-none');

                    // Hide Step 1, show loader
                    $('#step1').addClass('d-none');
                    $('#fetchLoader .screen-title').text('Authenticating session credentials…');
                    $('#ls1').html('<i class="bi bi-check-circle-fill"></i> Checking credential registry');
                    $('#ls2').html('<i class="bi bi-check-circle-fill"></i> Locating active credit files');
                    $('#ls3').html('<i class="bi bi-check-circle-fill"></i> Sending security verification code');
                    $('#ls1, #ls2, #ls3').removeClass('active done');
                    $('#fetchLoader').show();
                    $('html,body').animate({ scrollTop: 0 }, 200);

                    // Adapt times for simulation
                    setTimeout(function () {
                        $('#ls1').addClass('active');
                    }, 200);
                    setTimeout(function () {
                        $('#ls1').removeClass('active').addClass('done');
                        $('#ls2').addClass('active');
                    }, 900);
                    setTimeout(function () {
                        $('#ls2').removeClass('active').addClass('done');
                        $('#ls3').addClass('active');
                    }, 1800);
                    setTimeout(function () {
                        $('#ls3').removeClass('active').addClass('done');
                    }, 2700);

                    // Skip Step 2, show Step 3 (OTP)
                    setTimeout(function () {
                        $('#fetchLoader').hide();
                        $('#step3').removeClass('d-none');
                        if (isMobile) {
                            $('#otpEmailTarget').text('+91 ' + val.slice(0, 2) + '******' + val.slice(8));
                        } else {
                            $('#otpEmailTarget').text('registered contact details');
                        }

                        if (isUdyam) {
                            $('#dash_entUdyam').text(val);
                        } else {
                            $('#dash_entUdyam').text('UDYAM-MH-20-0012345');
                        }

                        startOtpTimer();
                        $('.otp-input').first().focus();
                        showToast('OTP sent to your registered contact.');
                        $('html,body').animate({ scrollTop: 0 }, 300);
                    }, 3200);

                } else {

                    const pattern = /^UDYAM-[A-Z]{2}-\d{2}-\d{7}$/;

                    if (!pattern.test(val)) {
                        $('#udyamError')
                            .removeClass('d-none')
                            .find('span')
                            .text('Please enter a valid Udyam number in the format UDYAM-XX-00-0000000.');
                        return;
                    }

                    $('#udyamError').addClass('d-none');

                    // Hide Step1 & Show Loader
                    $('#step1').addClass('d-none');

                    $('#fetchLoader .screen-title').text('Talking to the Udyam registry...');

                    $('#ls1').html('<i class="bi bi-check-circle-fill"></i> Validating Udyam number format');
                    $('#ls2').html('<i class="bi bi-check-circle-fill"></i> Fetching enterprise & PAN record');
                    $('#ls3').html('<i class="bi bi-check-circle-fill"></i> Saving MSME information');

                    $('#ls1,#ls2,#ls3').removeClass('active done');

                    $('#fetchLoader').show();

                    $('html,body').animate({ scrollTop: 0 }, 200);

                    // Animation
                    setTimeout(function () {
                        $('#ls1').addClass('active');
                    }, 200);

                    setTimeout(function () {
                        $('#ls1').removeClass('active').addClass('done');
                        $('#ls2').addClass('active');
                    }, 900);

                    setTimeout(function () {
                        $('#ls2').removeClass('active').addClass('done');
                        $('#ls3').addClass('active');
                    }, 1800);

                    // Call API
                    $.ajax({

                        url: "/Onboarding/FetchUdyam",
                        type: "POST",
                        data: {
                            uan: val
                        },

                        success: function (res) {

                            if (res.status) {

                                $('#ls3').removeClass('active').addClass('done');

                                loadUdyamDetails(res.uan)
                                    .done(function (response) {

                                        if (!response.status) {
                                            showToast(response.message);
                                            return;
                                        }

                                        var d = response.data;

                                        $("#rv_entName").text(d.enterpriseName);
                                        $("#rv_orgType").text(d.organizationType);
                                        $("#rv_dob").text(d.dateOfIncorporation);
                                        $("#rv_msme").text(d.enterpriseType);

                                        $("#rv_email").val(d.email);
                                        $("#mobileInput").val(d.mobile);

                                        $("#rv_address").text(d.address);

                                        // Show Step 2 only after data is loaded
                                        $('#fetchLoader').hide();
                                        $('#step1').addClass('d-none');
                                        $('#step2').removeClass('d-none');

                                        goToRailStep(2);

                                        showToast('Enterprise details fetched successfully.');

                                        $('html,body').animate({
                                            scrollTop: 0
                                        }, 300);
                                    })
                                    .fail(function () {

                                        $('#fetchLoader').hide();
                                        $('#step1').removeClass('d-none');

                                        showToast("Failed to load enterprise details.");
                                    });
                            }
                            
                            else {

                                $('#fetchLoader').hide();

                                $('#step1').removeClass('d-none');

                                showToast(res.message || "Unable to fetch Udyam details.");
                            }
                        },

                        error: function () {

                            $('#fetchLoader').hide();

                            $('#step1').removeClass('d-none');

                            showToast("Something went wrong while fetching Udyam details.");
                        }

                    });
                }

            });

            function loadUdyamDetails(uan) {

                return $.ajax({
                    url: '/Onboarding/GetUdyamDetails',
                    type: 'GET',
                    data: { uan: uan }
                });
            }

            // ---------- REVIEW STEP: enable Proceed button ----------
            function checkProceedEnabled() {
                const mobileOk = /^\d{10}$/.test($('#mobileInput').val().trim());
                const consentOk = $('#consentCheck').is(':checked');
                $('#btnProceedToOtp').prop('disabled', !(mobileOk && consentOk));
            }
            $('#mobileInput').on('input', function () {
                this.value = this.value.replace(/[^0-9]/g, '');
                checkProceedEnabled();
            });
            $('#consentCheck').on('change', checkProceedEnabled);

            // Back: Step 2 → Step 1
            $('#btnBackStep1').on('click', function () {
                $('#step2').addClass('d-none');
                $('#step1').removeClass('d-none');
                goToRailStep(1);
            });


            // ---------- STEP 2 → STEP 3 (send OTP) ----------
            $('#btnProceedToOtp').on('click', function () {

                var model = {
                    UdyamNumber: $('#rv_udyam_number').val(),   
                    MobileNumber: $('#mobileInput').val(),
                    Email: $('#rv_email').val(),
                    GstinNumber: $('#rv_gstnNumber').val(),     
                    PanNumber: $('#rv_panNo').val()             
                };

                const $btn = $(this);

                $btn.prop('disabled', true)
                    .html('<span class="spin-loader"></span> Registering...');

                $.ajax({
                    url: '/Onboarding/RegisterUser',
                    type: 'POST',
                    data: model,

                    success: function (response) {

                        if (response.status) {

                            $btn.prop('disabled', false)
                                .html('<i class="bi bi-send me-1"></i> Proceed & Send OTP');

                            $('#step2').addClass('d-none');
                            $('#step3').removeClass('d-none');

                            $('#otpEmailTarget').text(model.Email);

                            goToRailStep(3);
                            startOtpTimer();

                            $('.otp-input').first().focus();

                            showToast(response.message);

                            $('html,body').animate({ scrollTop: 0 }, 300);
                        }
                        else {
                            $('#reviewError').removeClass('d-none');
                            $('#reviewError span').text(response.message);

                            $btn.prop('disabled', false)
                                .html('<i class="bi bi-send me-1"></i> Register');
                        }
                    },

                    error: function () {

                        $('#reviewError').removeClass('d-none');
                        $('#reviewError span').text('Something went wrong.');

                        $btn.prop('disabled', false)
                            .html('<i class="bi bi-send me-1"></i> Register');
                    }
                });

            });



            // $('#btnProceedToOtp').on('click', function () {
            //     const $btn = $(this);
            //     $btn.prop('disabled', true).html('<span class="spin-loader"></span> Sending OTP…');
            //     setTimeout(function () {
            //         $btn.prop('disabled', false).html('<i class="bi bi-send me-1"></i> Proceed &amp; Send OTP');
            //         $('#step2').addClass('d-none');
            //         $('#step3').removeClass('d-none');
            //         $('#otpEmailTarget').text('co****ct@sharmaprecision.in');
            //         goToRailStep(3);
            //         startOtpTimer();
            //         $('.otp-input').first().focus();
            //         showToast('OTP sent to your registered email & mobile.');
            //         $('html,body').animate({ scrollTop: 0 }, 300);
            //     }, 1200);
            // });

            // ---------- OTP BOX BEHAVIOUR ----------
            $('.otp-input').on('input', function () {
                this.value = this.value.replace(/[^0-9]/g, '');
                if (this.value.length === 1) {
                    $(this).removeClass('is-invalid');
                    $('#otpError').addClass('d-none');
                    $(this).next('.otp-input').focus();
                }
            }).on('keydown', function (e) {
                if (e.key === 'Backspace' && this.value === '') {
                    $(this).prev('.otp-input').focus();
                }
            }).on('paste', function (e) {
                const text = (e.originalEvent.clipboardData || window.clipboardData).getData('text').replace(/[^0-9]/g, '');
                if (text.length) {
                    e.preventDefault();
                    const boxes = $('.otp-input');
                    for (let i = 0; i < boxes.length; i++) { boxes.eq(i).val(text[i] || ''); }
                    boxes.eq(Math.min(text.length, boxes.length) - 1).focus();
                }
            });

            let otpTimerInterval;
            function startOtpTimer() {
                let t = 60;
                $('#otpTimer').text('00:60');
                $('#resendOtp').addClass('disabled');
                clearInterval(otpTimerInterval);
                otpTimerInterval = setInterval(function () {
                    t--;
                    const s = String(t).padStart(2, '0');
                    $('#otpTimer').text('00:' + s);
                    if (t <= 0) {
                        clearInterval(otpTimerInterval);
                        $('#otpTimer').text('Expired');
                        $('#resendOtp').removeClass('disabled');
                    }
                }, 1000);
            }

            $('#resendOtp').on('click', function (e) {
                e.preventDefault();
                if ($(this).hasClass('disabled')) return;
                $('.otp-input').val('');
                $('.otp-input').first().focus();
                startOtpTimer();
                showToast('A new OTP has been sent.');
            });

            // Back: Step 3 → Step 2 / Step 1
            $('#btnBackStep2').on('click', function () {
                clearInterval(otpTimerInterval);
                $('#step3').addClass('d-none');
                if (isLoginMode) {
                    $('#step1').removeClass('d-none');
                } else {
                    $('#step2').removeClass('d-none');
                    goToRailStep(2);
                }
            });

            // ---------- STEP 3 → STEP 4 (verify OTP) ----------
            $('#btnVerifyOtp').on('click', function () {
                const code = $('.otp-input').map(function () { return this.value; }).get().join('');
                if (code.length < 6) {
                    $('#otpError').removeClass('d-none').html('<i class="bi bi-exclamation-triangle-fill me-1"></i> Please enter all 6 digits.');
                    $('.otp-input').filter(function () { return this.value === ''; }).addClass('is-invalid');
                    return;
                }
                const $btn = $(this);
                $btn.prop('disabled', true).html('<span class="spin-loader"></span> Verifying…');
                setTimeout(function () {
                    $btn.prop('disabled', false).html('<i class="bi bi-check-circle me-1"></i> Verify &amp; Continue');
                    clearInterval(otpTimerInterval);
                    $('#step3').addClass('d-none');
                    $('#step4').removeClass('d-none');

                    if (isLoginMode) {
                        const val = $('#udyamInput').val().trim();
                        if (/^UDYAM-[A-Z]{2}-\d{2}-\d{7}$/.test(val)) {
                            $('#entUdyam').text(val);
                            $('#dash_entUdyam').text(val);
                        } else {
                            $('#entUdyam').text('UDYAM-MH-20-0012345');
                            $('#dash_entUdyam').text('UDYAM-MH-20-0012345');
                        }
                        $('#entName').text('Sharma Precision Engineering Works');
                        $('#dash_entName').text('Sharma Precision Engineering Works');
                    } else {
                        $('#entName').text($('#rv_entName').text());
                        $('#dash_entName').text($('#rv_entName').text());
                        $('#entUdyam').text($('#udyamInput').val());
                        $('#dash_entUdyam').text($('#udyamInput').val());
                        goToRailStep(4);
                    }

                    showToast('Identity verified successfully.');
                    $('html,body').animate({ scrollTop: 0 }, 300);
                }, 1200);
            });

            // ---------- Dashboard button ----------
            $('#btnGoToDashboard').on('click', function () {
                showToast('Welcome to your Credit Risk Intelligence Dashboard.');
                $('#onboardingFlow').addClass('d-none');
                $('#dashboardView').removeClass('d-none');
                $('html,body').animate({ scrollTop: 0 }, 300);
            });

        });