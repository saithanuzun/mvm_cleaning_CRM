// src/pages/BookingPage.jsx
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

const BookingPage = ({ bookingData, updateBookingData }) => {
    const navigate = useNavigate();

    const [customerDetails, setCustomerDetails] = useState({
        name: '',
        email: '',
        address: '',
        city: '',
        postcode: bookingData.postcode || '',
        notes: ''
    });
    const [paymentMethod, setPaymentMethod] = useState('cash'); // 'cash' or 'card'
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [promoCode, setPromoCode] = useState(bookingData.appliedPromotionCode || '');
    const [promoDiscount, setPromoDiscount] = useState(0);
    const [promoError, setPromoError] = useState('');
    const [promoApplied, setPromoApplied] = useState(bookingData.promotionApplied || false);

    // Redirect if incomplete booking data
    useEffect(() => {
        if (!bookingData.postcode || (!bookingData.selectedServicesData && !bookingData.basket) || !bookingData.selectedTimeSlot) {
            navigate('/');
        }
    }, [bookingData, navigate]);

    // Block browser back button when a promo has been applied
    useEffect(() => {
        if (!promoApplied) return;

        // Push a duplicate history entry so the first back press hits our sentinel
        window.history.pushState({ promoLock: true }, '');

        const handlePopState = () => {
            // Re-push to keep user on this page
            window.history.pushState({ promoLock: true }, '');
        };

        window.addEventListener('popstate', handlePopState);
        return () => window.removeEventListener('popstate', handlePopState);
    }, [promoApplied]);

    const handleInputChange = (e) => {
        setCustomerDetails({
            ...customerDetails,
            [e.target.name]: e.target.value
        });
    };

    const validateForm = () => {
        if (!customerDetails.name.trim()) return 'Name is required';
        if (!customerDetails.email.trim()) return 'Email is required';
        if (!customerDetails.address.trim()) return 'Address is required';
        if (!customerDetails.city.trim()) return 'City is required';
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(customerDetails.email)) return 'Invalid email address';
        return null;
    };

    // Calculate multi-service discount savings
    const calculateMultiServiceDiscount = () => {
        const services = bookingData.selectedServicesData || bookingData.basket?.items || [];
        if (services.length === 0) return 0;

        let originalTotal = 0;
        let discountedTotal = 0;

        services.forEach((item, index) => {
            const discountRate = index === 0 ? 0 : index === 1 ? 0.10 : 0.20;
            const originalPrice = item.price || 0;
            const discountedPrice = originalPrice * (1 - discountRate);

            originalTotal += originalPrice * item.quantity;
            discountedTotal += discountedPrice * item.quantity;
        });

        return originalTotal - discountedTotal;
    };

    const handleApplyPromo = async () => {
        setPromoError('');
        setPromoDiscount(0);

        if (!promoCode.trim()) {
            setPromoError('Please enter a promo code');
            return;
        }

        if (!bookingData.bookingId) {
            setPromoError('Booking ID not found. Please complete your booking first.');
            return;
        }

        try {
            // Call the API to apply promotion
            const response = await api.booking.applyPromotion(bookingData.bookingId, promoCode.toUpperCase());

            if (!response.success) {
                setPromoError(response.message || 'Invalid promo code');
                setPromoApplied(false);
                return;
            }

            const discountAmount = response.data?.discountAmount || 0;
            setPromoDiscount(discountAmount);
            setPromoApplied(true);
            setPaymentMethod('card'); // Force card payment when promo is applied
            setPromoError('');
            // Persist the promotion state into shared bookingData
            updateBookingData({ promotionApplied: true, appliedPromotionCode: promoCode.toUpperCase() });
        } catch (error) {
            setPromoError(error.message || 'Error applying promo code. Please try again.');
            setPromoApplied(false);
        }
    };

    const handleRemovePromo = () => {
        setPromoCode('');
        setPromoDiscount(0);
        setPromoApplied(false);
        setPaymentMethod('cash'); // Reset to cash when promo is removed
        setPromoError('');
    };

    const createDateTime = (date, timeString) => {
        // Parse time string (e.g., "09:00" or "09:00:00")
        const [hours, minutes] = timeString.split(':').map(Number);

        // Create date in UTC (not local time)
        const pad = (n) => String(n).padStart(2, '0');
        const year = date.getFullYear();
        const month = pad(date.getMonth() + 1);
        const day = pad(date.getDate());
        const hour = pad(hours);
        const minute = pad(minutes);

        // Send as UTC ISO string
        return `${year}-${month}-${day}T${hour}:${minute}:00Z`;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        const validationError = validateForm();
        if (validationError) {
            setError(validationError);
            window.scrollTo({ top: 0, behavior: 'smooth' });
            return;
        }

        setLoading(true);
        setError('');

        try {
            // Prepare booking data for API
            const servicesArray = bookingData.selectedServicesData || bookingData.basket?.items || [];
            const bookingRequest = {
                bookingId: bookingData.bookingId,
                customerName: customerDetails.name,
                customerEmail: customerDetails.email,
                customerPhone: bookingData.phone,
                address: `${customerDetails.address}, ${customerDetails.city}`,
                postcode: bookingData.postcode,
                contractorId: bookingData.contractorId,
                scheduledSlot: {
                    startTime: createDateTime(bookingData.selectedDate, bookingData.selectedTimeSlot.startTime),
                    endTime: createDateTime(bookingData.selectedDate, bookingData.selectedTimeSlot.endTime)
                },
                services: servicesArray.map(item => ({
                    serviceId: item.id || item.serviceId,
                    serviceName: item.name || item.serviceName,
                    quantity: item.quantity,
                    price: item.price
                })),
                totalAmount: bookingData.totalAmount,
                paymentMethod: paymentMethod
            };

            // Add notes to address if provided
            if (customerDetails.notes) {
                bookingRequest.address += ` (Notes: ${customerDetails.notes})`;
            }

            // Complete booking with payment method handling
            const response = await api.booking.complete(bookingRequest);

            if (response.success && response.data) {
                // Store booking ID and details for success page
                localStorage.setItem('pending_booking_id', response.data.bookingId);
                localStorage.setItem('booking_data', JSON.stringify({
                    bookingId: response.data.bookingId,
                    status: response.data.status,
                    customerName: customerDetails.name,
                    customerEmail: customerDetails.email,
                    paymentMethod: paymentMethod
                }));



                // Update booking data
                updateBookingData({
                    customerDetails,
                    bookingId: response.data.bookingId,
                    paymentMethod: paymentMethod
                });
                // ====================  ====================
                // Trigger Google Ads conversion tracking
                if (window.gtag) {
                    window.gtag('event', 'conversion', {
                        'send_to': 'AW-17424866501/uEZiCM-QzP0aEMW56fRA',
                        'value': bookingData.totalAmount,
                        'currency': 'GBP',
                        'transaction_id': response.data.bookingId || '',
                        // Uncomment and populate if you track new customers:
                        // 'new_customer': true/false
                    });
                }
                // ======================================================

                // If cash payment, redirect to success page
                if (paymentMethod === 'cash') {
                    navigate('/payment-success');
                } else {
                    // If card payment, redirect to Stripe payment URL
                    window.location.href = response.data.paymentUrl;
                }
            } else {
                setError(response.message || 'Failed to create booking. Please try again.');
            }
        } catch (err) {
            console.error('Booking creation error:', err);
            setError(err.response?.data?.message || 'Failed to create booking. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    const formatTime = (timeString) => {
        return new Date(`1970-01-01T${timeString}`).toLocaleTimeString('en-GB', {
            hour: '2-digit',
            minute: '2-digit',
            hour12: true
        });
    };

    const formatDateFull = (date) => {
        if (!date) return '';
        return date.toLocaleDateString('en-GB', {
            weekday: 'long',
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        });
    };

    return (
        <div className="min-h-screen bg-gradient-to-br from-gray-50 via-white to-blue-50/30 py-8 px-4">
            <div className="max-w-4xl mx-auto">
                {/* Header */}
                <div className="text-center mb-8">
                    <div className="inline-block px-4 py-2 rounded-full mb-4" style={{ backgroundColor: '#46C6CE20' }}>
                        <span className="font-bold text-sm" style={{ color: '#194376' }}>Step 4 of 4</span>
                    </div>
                    <h1 className="text-4xl font-bold mb-2" style={{ color: '#194376' }}>
                        Complete Your Booking
                    </h1>
                    <p className="text-gray-600 text-lg">
                        Enter your details to proceed to payment
                    </p>
                </div>

                {/* Error Message */}
                {error && (
                    <div className="bg-red-50 border-l-4 border-red-500 rounded-lg p-4 mb-8 animate-in fade-in">
                        <div className="flex items-start">
                            <svg className="h-5 w-5 text-red-500 mr-3 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                            <p className="text-red-700 font-medium">{error}</p>
                        </div>
                    </div>
                )}

                {/* Booking Summary */}
                <div className="bg-white rounded-2xl p-6 mb-8 border-2 shadow-lg" style={{ borderColor: '#46C6CE' }}>
                    <div className="flex items-center mb-4">
                        <div className="w-10 h-10 rounded-lg flex items-center justify-center mr-3" style={{ background: '#194376' }}>
                            <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                            </svg>
                        </div>
                        <h3 className="text-xl font-bold text-gray-800">Booking Summary</h3>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div className="bg-gray-50 p-4 rounded-xl md:col-span-2">
                            <p className="text-xs text-gray-500 mb-3 font-semibold uppercase tracking-wide">Services Selected</p>
                            <div className="space-y-2">
                                {(bookingData.selectedServicesData || bookingData.basket?.items) && (bookingData.selectedServicesData || bookingData.basket?.items).length > 0 ? (
                                    (bookingData.selectedServicesData || bookingData.basket.items).map((item, index) => {
                                        const discountRate = index === 0 ? 0 : index === 1 ? 0.10 : 0.20;
                                        const originalPrice = (item.price || 0) * item.quantity;
                                        const discountedPrice = originalPrice * (1 - discountRate);
                                        const hasDiscount = discountRate > 0;

                                        return (
                                            <div key={index} className="flex justify-between items-center p-2 bg-white rounded">
                                                <div>
                                                    <p className="font-semibold text-gray-800 text-sm">{item.name || item.serviceName}</p>
                                                    <p className="text-xs text-gray-500">Qty: {item.quantity}</p>
                                                </div>
                                                <div className="text-right">
                                                    {hasDiscount ? (
                                                        <div>
                                                            <p className="text-xs text-gray-400 line-through">£{originalPrice.toFixed(2)}</p>
                                                            <p className="font-bold text-green-600">£{discountedPrice.toFixed(2)}</p>
                                                            <span className="text-xs bg-green-100 text-green-700 px-1 py-0.5 rounded font-bold">
                                                                -{(discountRate * 100).toFixed(0)}% off
                                                            </span>
                                                        </div>
                                                    ) : (
                                                        <p className="font-bold text-gray-800">£{originalPrice.toFixed(2)}</p>
                                                    )}
                                                </div>
                                            </div>
                                        );
                                    })
                                ) : (
                                    <p className="text-sm text-gray-600">No services selected</p>
                                )}
                            </div>
                        </div>
                        <div className="bg-gray-50 p-4 rounded-xl">
                            <p className="text-xs text-gray-500 mb-1 font-semibold uppercase tracking-wide">Date & Time</p>
                            <p className="font-bold text-gray-800 text-sm">
                                {bookingData.selectedDate && bookingData.selectedTimeSlot && (
                                    <>
                                        {formatDateFull(bookingData.selectedDate)}
                                        <br />
                                        {formatTime(bookingData.selectedTimeSlot.startTime)} - {formatTime(bookingData.selectedTimeSlot.endTime)}
                                    </>
                                )}
                            </p>
                        </div>
                        <div className="bg-gray-50 p-4 rounded-xl">
                            <p className="text-xs text-gray-500 mb-1 font-semibold uppercase tracking-wide">Location</p>
                            <p className="font-bold text-gray-800">{bookingData.postcode}</p>
                            <p className="text-xs text-gray-600 mt-1">Phone: {bookingData.phone}</p>
                        </div>
                        <div className="bg-gray-50 p-4 rounded-xl md:col-span-2">
                            <div className="flex justify-between items-center">
                                <div>
                                    <p className="text-xs text-gray-500 font-semibold uppercase tracking-wide">Contractor</p>
                                    <p className="font-bold text-gray-800">{bookingData.contractorName || 'Assigned'}</p>
                                </div>
                                <div className="text-right">
                                    <p className="text-xs text-gray-500 font-semibold uppercase tracking-wide">Total</p>
                                    <div>
                                        <p className="font-bold text-2xl" style={{ color: '#194376' }}>
                                            £{(bookingData.totalAmount && promoDiscount > 0
                                                ? (bookingData.totalAmount - promoDiscount).toFixed(2)
                                                : bookingData.totalAmount?.toFixed(2)) || '0.00'}
                                        </p>
                                        {calculateMultiServiceDiscount() > 0 && (
                                            <p className="text-xs text-green-600 font-semibold">
                                                Multi-service discount: £{calculateMultiServiceDiscount().toFixed(2)}
                                            </p>
                                        )}
                                        {promoDiscount > 0 && (
                                            <p className="text-xs text-green-600 font-semibold">
                                                Promo code saving: £{promoDiscount.toFixed(2)}
                                            </p>
                                        )}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <form onSubmit={handleSubmit} className="space-y-8">
                    {/* Customer Details Section */}
                    <div className="bg-white rounded-2xl shadow-lg p-6 border-2 border-gray-100">
                        <h3 className="text-2xl font-bold text-gray-800 mb-6 flex items-center">
                            <svg className="w-6 h-6 mr-2" style={{ color: '#194376' }} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                            </svg>
                            Your Details
                        </h3>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                            <div>
                                <label className="block text-gray-700 text-sm font-bold mb-2">
                                    Full Name *
                                </label>
                                <input
                                    type="text"
                                    name="name"
                                    value={customerDetails.name}
                                    onChange={handleInputChange}
                                    className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:border-[#46C6CE] focus:ring-2 focus:ring-[#46C6CE]/20 outline-none transition-all"
                                    placeholder="John Smith"
                                    required
                                />
                            </div>

                            <div>
                                <label className="block text-gray-700 text-sm font-bold mb-2">
                                    Email Address *
                                </label>
                                <input
                                    type="email"
                                    name="email"
                                    value={customerDetails.email}
                                    onChange={handleInputChange}
                                    className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:border-[#46C6CE] focus:ring-2 focus:ring-[#46C6CE]/20 outline-none transition-all"
                                    placeholder="john@example.com"
                                    required
                                />
                            </div>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                            <div>
                                <label className="block text-gray-700 text-sm font-bold mb-2">
                                    Address *
                                </label>
                                <input
                                    type="text"
                                    name="address"
                                    value={customerDetails.address}
                                    onChange={handleInputChange}
                                    className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:border-[#46C6CE] focus:ring-2 focus:ring-[#46C6CE]/20 outline-none transition-all"
                                    placeholder="123 Main Street"
                                    required
                                />
                            </div>

                            <div>
                                <label className="block text-gray-700 text-sm font-bold mb-2">
                                    City *
                                </label>
                                <input
                                    type="text"
                                    name="city"
                                    value={customerDetails.city}
                                    onChange={handleInputChange}
                                    className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:border-[#46C6CE] focus:ring-2 focus:ring-[#46C6CE]/20 outline-none transition-all"
                                    placeholder="London"
                                    required
                                />
                            </div>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <label className="block text-gray-700 text-sm font-bold mb-2">
                                    Postcode
                                </label>
                                <input
                                    type="text"
                                    name="postcode"
                                    value={customerDetails.postcode}
                                    className="w-full px-4 py-3 border-2 border-gray-200 rounded-xl bg-gray-50 text-gray-600"
                                    readOnly
                                />
                            </div>

                            <div>
                                <label className="block text-gray-700 text-sm font-bold mb-2">
                                    Special Instructions (Optional)
                                </label>
                                <textarea
                                    name="notes"
                                    value={customerDetails.notes}
                                    onChange={handleInputChange}
                                    className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:border-[#46C6CE] focus:ring-2 focus:ring-[#46C6CE]/20 outline-none transition-all"
                                    placeholder="Any special instructions..."
                                    rows="3"
                                />
                            </div>
                        </div>
                    </div>

                    {/* Promo Code Section */}
                    <div className="bg-white rounded-2xl shadow-lg p-6 border-2 border-gray-100">
                        <h3 className="text-2xl font-bold text-gray-800 mb-6 flex items-center">
                            <svg className="w-6 h-6 mr-2" style={{ color: '#194376' }} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 5v2m0 4v2m0 4v2M5 5a2 2 0 012-2h6a2 2 0 012 2m0 16H7a2 2 0 01-2-2m16 0a2 2 0 01-2 2h-6a2 2 0 01-2-2m0-4V9m0 4V5m0 16h6a2 2 0 002-2V5a2 2 0 00-2-2h-6a2 2 0 00-2 2" />
                            </svg>
                            Promo Code
                        </h3>

                        {promoApplied ? (
                            <div className="bg-green-50 border-2 border-green-400 rounded-xl p-4 mb-4">
                                <div className="flex items-start justify-between">
                                    <div className="flex items-start">
                                        <svg className="w-5 h-5 text-green-600 mr-3 mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                                            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                        </svg>
                                        <div>
                                            <p className="font-bold text-green-800">Promo Code Applied!</p>
                                            <p className="text-sm text-green-700 mt-1">
                                                Code <span className="font-mono font-bold">{promoCode.toUpperCase()}</span> - Save £{promoDiscount.toFixed(2)}
                                            </p>
                                        </div>
                                    </div>
                                    <button
                                        type="button"
                                        onClick={handleRemovePromo}
                                        className="text-green-600 hover:text-green-800 font-semibold text-sm"
                                    >
                                        Remove
                                    </button>
                                </div>
                            </div>
                        ) : (
                            <div className="flex gap-3">
                                <div className="flex-1">
                                    <input
                                        type="text"
                                        value={promoCode}
                                        onChange={(e) => {
                                            setPromoCode(e.target.value);
                                            setPromoError('');
                                        }}
                                        placeholder="Enter promo code (e.g., SAVE10)"
                                        className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:border-[#46C6CE] focus:ring-2 focus:ring-[#46C6CE]/20 outline-none transition-all uppercase"
                                    />
                                    {promoError && (
                                        <p className="text-red-600 text-sm mt-2 flex items-center gap-1">
                                            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                                            </svg>
                                            {promoError}
                                        </p>
                                    )}
                                </div>
                                <button
                                    type="button"
                                    onClick={handleApplyPromo}
                                    className="px-8 py-3 bg-[#46C6CE] text-white font-bold rounded-xl hover:bg-[#3ba4ac] transition-all flex items-center gap-2 whitespace-nowrap"
                                >
                                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 4v16m8-8H4" />
                                    </svg>
                                    Apply
                                </button>
                            </div>
                        )}

                        <p className="text-xs text-gray-500 mt-4 italic">
                            💡 Try these codes: <span className="font-mono font-semibold">SAVE10</span> (10% off), or <span className="font-mono font-semibold">WELCOME5</span> (5% off)
                        </p>
                        <p className="text-xs text-amber-600 mt-2 italic font-medium">
                            ⚠️ Promotion codes can only be applied to card payments. Cash payments will be disabled when using a promo code.
                        </p>
                    </div>

                    {/* Payment Method Selection */}
                    <div className="bg-white rounded-2xl shadow-lg p-6 border-2 border-gray-100">
                        <h3 className="text-2xl font-bold text-gray-800 mb-6 flex items-center">
                            <svg className="w-6 h-6 mr-2" style={{ color: '#194376' }} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M3 10h18M3 14h18m-9-4v8m-7 0a2 2 0 01-2-2V8a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2H5z" />
                            </svg>
                            Payment Method
                        </h3>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {/* Cash Option */}
                            <div
                                onClick={() => !promoApplied && setPaymentMethod('cash')}
                                className={`
                                p-4 rounded-xl border-2 cursor-pointer transition-all
                                ${promoApplied
                                        ? 'border-gray-200 bg-gray-100 opacity-50 cursor-not-allowed'
                                        : paymentMethod === 'cash'
                                            ? 'border-[#194376] bg-blue-50'
                                            : 'border-gray-300 bg-gray-50 hover:border-gray-400'
                                    }
                            `}
                            >
                                <div className="flex items-start">
                                    <input
                                        type="radio"
                                        name="paymentMethod"
                                        value="cash"
                                        checked={paymentMethod === 'cash'}
                                        onChange={(e) => !promoApplied && setPaymentMethod(e.target.value)}
                                        disabled={promoApplied}
                                        className="mt-1 h-5 w-5 cursor-pointer"
                                        style={{ accentColor: '#194376' }}
                                    />
                                    <div className="ml-4 flex-1">
                                        <h4 className="font-bold text-gray-800 text-lg">Pay on Completion</h4>
                                        <p className="text-sm text-gray-600 mt-1">
                                            Pay in cash after the service is completed
                                        </p>
                                        {promoApplied && (
                                            <p className="text-xs text-red-600 mt-2 font-semibold">
                                                💳 Card payment required with promo codes
                                            </p>
                                        )}
                                        <p className="text-xs text-gray-500 mt-2 font-semibold">
                                            Total: £{(bookingData.totalAmount && promoDiscount > 0
                                                ? (bookingData.totalAmount - promoDiscount).toFixed(2)
                                                : bookingData.totalAmount?.toFixed(2)) || '0.00'}
                                        </p>
                                    </div>
                                </div>
                            </div>

                            {/* Card Option */}
                            <div
                                onClick={() => setPaymentMethod('card')}
                                className={`
                                p-4 rounded-xl border-2 cursor-pointer transition-all
                                ${paymentMethod === 'card'
                                        ? 'border-[#46C6CE] bg-cyan-50'
                                        : 'border-gray-300 bg-gray-50 hover:border-gray-400'
                                    }
                            `}
                            >
                                <div className="flex items-start">
                                    <input
                                        type="radio"
                                        name="paymentMethod"
                                        value="card"
                                        checked={paymentMethod === 'card'}
                                        onChange={(e) => setPaymentMethod(e.target.value)}
                                        className="mt-1 h-5 w-5 cursor-pointer"
                                        style={{ accentColor: '#46C6CE' }}
                                    />
                                    <div className="ml-4 flex-1">
                                        <h4 className="font-bold text-gray-800 text-lg">Pay by Card Now</h4>
                                        <p className="text-sm text-gray-600 mt-1">
                                            Secure payment via Stripe (card, Apple Pay, Google Pay)
                                        </p>
                                        <p className="text-xs text-gray-500 mt-2 font-semibold">
                                            Total: £{(bookingData.totalAmount && promoDiscount > 0
                                                ? (bookingData.totalAmount - promoDiscount).toFixed(2)
                                                : bookingData.totalAmount?.toFixed(2)) || '0.00'}
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {paymentMethod === 'cash' && (
                            <div className="mt-4 p-4 rounded-lg bg-blue-50 border-l-4 border-blue-400">
                                <p className="text-sm text-blue-800">
                                    <span className="font-semibold">Note:</span> You will pay the contractor in cash when they arrive to complete the service.
                                </p>
                            </div>
                        )}
                    </div>

                    {/* Terms and Conditions */}
                    <div className="bg-gradient-to-r from-blue-50 to-cyan-50 rounded-xl p-6 border-2" style={{ borderColor: '#46C6CE40' }}>
                        <div className="flex items-start">
                            <input
                                type="checkbox"
                                id="terms"
                                required
                                className="mt-1 mr-3 h-5 w-5 rounded border-2 focus:ring-2 focus:ring-offset-2"
                                style={{ accentColor: '#194376', borderColor: '#194376' }}
                            />
                            <label htmlFor="terms" className="text-sm text-gray-700 leading-relaxed">
                                I agree to the Terms of Service and understand that this booking is subject to availability confirmation.
                                {paymentMethod === 'card' && ' I authorize the charge for the selected services and understand that payment will be processed securely via Stripe.'}
                                {paymentMethod === 'cash' && ' I agree to pay the contractor in cash upon completion of the service.'}
                            </label>
                        </div>
                    </div>

                    {/* Navigation Buttons */}
                    <div className="flex flex-col sm:flex-row justify-between items-center gap-4 pt-6 border-t-2 border-gray-200">
                        <div className="relative group w-full sm:w-auto">
                            <button
                                type="button"
                                onClick={() => !promoApplied && navigate('/time-slots')}
                                disabled={promoApplied}
                                className={`px-8 py-4 border-2 font-bold rounded-xl transition-all w-full sm:w-auto flex items-center justify-center gap-2
                                    ${promoApplied
                                        ? 'border-gray-200 bg-gray-100 text-gray-400 cursor-not-allowed opacity-60'
                                        : 'border-gray-300 text-gray-700 hover:bg-gray-50 hover:border-gray-400 cursor-pointer'
                                    }`}
                            >
                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 19l-7-7 7-7" />
                                </svg>
                                Back to Time Slots
                            </button>
                            {promoApplied && (
                                <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-3 py-1.5 bg-gray-800 text-white text-xs rounded-lg whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none">
                                    🔒 Promo applied — navigation is locked
                                </div>
                            )}
                        </div>

                        <button
                            type="submit"
                            disabled={loading}
                            className={`
                                px-8 py-4 font-bold rounded-xl transition-all w-full sm:w-auto flex items-center justify-center gap-2
                                ${loading
                                    ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                                    : 'bg-[#194376] text-white hover:shadow-2xl transform hover:scale-105 active:scale-95'
                                }
                            `}
                        >
                            {loading ? (
                                <>
                                    <svg className="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                    </svg>
                                    Creating Booking...
                                </>
                            ) : (
                                <>
                                    {paymentMethod === 'cash' ? 'Confirm Booking' : 'Proceed to Payment'}
                                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M13 7l5 5m0 0l-5 5m5-5H6" />
                                    </svg>
                                </>
                            )}
                        </button>
                    </div>
                </form>

                {/* Payment Info */}
                <div className="mt-6 text-center">
                    <div className="inline-flex items-center gap-2 text-sm text-gray-600">
                        <svg className="w-4 h-4 text-[#46C6CE]" fill="currentColor" viewBox="0 0 20 20">
                            <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
                        </svg>
                        Secure payment powered by Stripe
                    </div>
                </div>
            </div>
        </div>
    );
};

export default BookingPage;
