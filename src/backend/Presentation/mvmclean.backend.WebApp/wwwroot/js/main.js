(function ($) {
    "use strict";

    // Dropdown on mouse hover
    $(document).ready(function () {
        function toggleNavbarMethod() {
            if ($(window).width() > 992) {
                $('.navbar .dropdown').on('mouseover', function () {
                    $('.dropdown-toggle', this).trigger('click');
                }).on('mouseout', function () {
                    $('.dropdown-toggle', this).trigger('click').blur();
                });
            } else {
                $('.navbar .dropdown').off('mouseover').off('mouseout');
            }
        }
        toggleNavbarMethod();
        $(window).resize(toggleNavbarMethod);
    });


    // Back to top button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 100) {
            $('.back-to-top').fadeIn('slow');
        } else {
            $('.back-to-top').fadeOut('slow');
        }
    });
    $('.back-to-top').click(function () {
        $('html, body').animate({scrollTop: 0}, 1500, 'easeInOutExpo');
        return false;
    });


    // Facts counter
    $('[data-toggle="counter-up"]').counterUp({
        delay: 10,
        time: 2000
    });


    // Testimonials carousel
    if ($.fn.owlCarousel) {
        $(".testimonial-carousel").owlCarousel({
            autoplay: true,
            smartSpeed: 1000,
            dots: true,
            loop: true,
            margin: 30,
            responsive: {
                0:{ items:1 },
                576:{ items:1 },
                768:{ items:2 },
                992:{ items:3 }
            }
        });
    }

    // Trusted businesses carousel – same behaviour as testimonial carousel
    var $trustedBusinesses = $(".trusted-business-carousel");

    if ($.fn.owlCarousel) {
        $trustedBusinesses.owlCarousel({
            autoplay: true,
            smartSpeed: 1000,
            dots: true,
            loop: true,
            margin: 30,
            responsive: {
                0:   { items: 1 },
                576: { items: 1 },
                768: { items: 2 },
                992: { items: 3 }
            }
        });
    } else {
        // Fallback: swipeable scroll if Owl is unavailable
        $trustedBusinesses.addClass("trusted-business-carousel--fallback");
        var trustedBusinessTimer;
        var advanceTrustedBusinesses = function () {
            var rail = $trustedBusinesses.get(0);
            if (!rail || rail.matches(":hover") || rail.matches(":focus-within")) return;
            var nextLeft = rail.scrollLeft + rail.clientWidth * 0.8;
            if (nextLeft + rail.clientWidth >= rail.scrollWidth) nextLeft = 0;
            rail.scrollTo({ left: nextLeft, behavior: "smooth" });
        };
        trustedBusinessTimer = window.setInterval(advanceTrustedBusinesses, 2500);
        $trustedBusinesses.on("mouseenter focusin", function () {
            window.clearInterval(trustedBusinessTimer);
        }).on("mouseleave focusout", function () {
            window.clearInterval(trustedBusinessTimer);
            trustedBusinessTimer = window.setInterval(advanceTrustedBusinesses, 2500);
        });
    }


    // Related Post carousel
    if ($.fn.owlCarousel) {
        $(".related-carousel").owlCarousel({
            autoplay: true,
            smartSpeed: 1000,
            dots: true,
            loop: true,
            margin: 30,
            responsive: {
                0:{ items:1 },
                576:{ items:1 },
                768:{ items:2 }
            }
        });
    }

})(jQuery);
