import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

const ServicesPage = ({ bookingData, updateBookingData }) => {
    const [services, setServices] = useState([]);
    const [selectedServices, setSelectedServices] = useState(bookingData.selectedServices || {});
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [activeCategory, setActiveCategory] = useState('all');
    const [categories, setCategories] = useState([]);
    const navigate = useNavigate();

    useEffect(() => {
        // Redirect if no postcode
        if (!bookingData.postcode) {
            navigate('/postcode');
            return;
        }

        // Fetch services by postcode from API
        const fetchServices = async () => {
            try {
                setLoading(true);
                setError('');

                const response = await api.services.getByPostcode(bookingData.postcode);

                if (response && response.success && response.data && Array.isArray(response.data)) {
                    // Map API response to service object structure
                    const mappedServices = response.data.map(service => ({
                        id: service.serviceId,
                        name: service.serviceName,
                        description: service.description,
                        price: service.adjustedPrice,
                        duration: service.estimatedDurationMinutes,
                        category: service.category.toLowerCase().replace(/\s+/g, '-')
                    }));

                    setServices(mappedServices);

                    // Extract unique categories from services and sort in desired order
                    const uniqueCategories = [...new Set(mappedServices.map(s => s.category))];

                    // Define priority order for categories (start with these)
                    const startPriority = [
                        'carpet-cleaning',
                        'upholstery-cleaning',
                        'cleaning-packages',
                    ];

                    // Define categories that should be at the end
                    const endPriority = [
                        'additional-services',
                        'all-services',
                    ];

                    // Sort categories: priority first, then middle categories, then end categories
                    const sortedCategories = uniqueCategories.sort((a, b) => {
                        const startIndexA = startPriority.indexOf(a);
                        const startIndexB = startPriority.indexOf(b);
                        const endIndexA = endPriority.indexOf(a);
                        const endIndexB = endPriority.indexOf(b);

                        // Both in start priority
                        if (startIndexA !== -1 && startIndexB !== -1) return startIndexA - startIndexB;
                        // Only a is in start priority
                        if (startIndexA !== -1) return -1;
                        // Only b is in start priority
                        if (startIndexB !== -1) return 1;

                        // Both in end priority
                        if (endIndexA !== -1 && endIndexB !== -1) return endIndexA - endIndexB;
                        // Only a is in end priority
                        if (endIndexA !== -1) return 1;
                        // Only b is in end priority
                        if (endIndexB !== -1) return -1;

                        // Both are middle categories - sort alphabetically
                        return a.localeCompare(b);
                    });

                    const categoryList = [
                        { id: 'all', name: 'All Services', color: '#46C6CE' },
                        ...sortedCategories.map(cat => ({
                            id: cat,
                            name: cat.replace('-', ' ').split(' ').map(word => word.charAt(0).toUpperCase() + word.slice(1)).join(' '),
                            color: '#46C6CE'
                        }))
                    ];
                    setCategories(categoryList);
                } else {
                    setError('No services available for your area');
                }
            } catch (err) {
                console.error('Error fetching services:', err);
                setError('Failed to load services. Please try again.');
            } finally {
                setLoading(false);
            }
        };

        fetchServices();
    }, [bookingData.postcode, navigate]);

    useEffect(() => {
        // Initialize selectedServices from bookingData
        if (bookingData.selectedServices) {
            setSelectedServices(bookingData.selectedServices);
        }
    }, [bookingData.selectedServices]);

    const handleQuantityChange = async (serviceId, change) => {
        try {
            setError('');
            const currentQty = selectedServices[serviceId] || 0;
            const newQty = Math.max(0, currentQty + change);

            if (newQty === 0) {
                // Remove from cart when quantity reaches 0
                const response = await api.basket.remove(bookingData.bookingId, serviceId);
                console.log('Remove response:', response);

                if (response && response.success) {
                    setSelectedServices(prev => {
                        const { [serviceId]: removed, ...rest } = prev;
                        return rest;
                    });
                } else {
                    setError(response?.message || 'Failed to remove service');
                }
            } else if (change === 1) {
                // Add one item to cart
                const service = services.find(s => s.id === serviceId);
                if (!service) return;

                const response = await api.basket.add(
                    bookingData.bookingId,
                    serviceId,
                    service.name,
                    service.price,
                    service.duration
                );
                console.log('Add response:', response);

                if (response && response.success) {
                    setSelectedServices(prev => ({
                        ...prev,
                        [serviceId]: newQty
                    }));
                } else {
                    setError(response?.message || 'Failed to add service');
                }
            } else if (change === -1) {
                // Decrement one item from cart
                const response = await api.basket.remove(bookingData.bookingId, serviceId);
                console.log('Remove response:', response);

                if (response && response.success) {
                    setSelectedServices(prev => ({
                        ...prev,
                        [serviceId]: newQty
                    }));
                } else {
                    setError(response?.message || 'Failed to remove service');
                }
            }
        } catch (err) {
            console.error('Error updating cart:', err);
            setError(err.response?.data?.message || 'Failed to update cart. Please try again.');
        }
    };
    const handleAddService = (serviceId) => {
        handleQuantityChange(serviceId, 1);
    };


    const calculateTotal = () => {
        const selectedServicesList = Object.entries(selectedServices)
            .map(([serviceId, quantity]) => {
                const service = services.find(s => s.id === serviceId);
                return service ? { ...service, quantity } : null;
            })
            .filter(s => s !== null);

        let total = 0;
        let unitCounter = 0;
        selectedServicesList.forEach((service) => {
            for (let i = 0; i < service.quantity; i++) {
                const discountRate = unitCounter === 0 ? 0 : unitCounter === 1 ? 0.10 : 0.20;
                total += service.price * (1 - discountRate);
                unitCounter++;
            }
        });

        return total;
    };

    const calculateTotalDuration = () => {
        return Object.entries(selectedServices).reduce((total, [serviceId, quantity]) => {
            const service = services.find(s => s.id === serviceId);
            return total + (service?.duration || 0) * quantity;
        }, 0);
    };

    const getSelectedServicesCount = () => {
        return Object.values(selectedServices).reduce((sum, qty) => sum + qty, 0);
    };

    const getFilteredServices = () => {
        const filtered = activeCategory === 'all' ? services : services.filter(service => service.category === activeCategory);

        // Define priority order for categories (start with these)
        const startPriority = [
            'carpet-cleaning',
            'upholstery-cleaning',
            'cleaning-packages',
        ];

        // Define categories that should be at the end
        const endPriority = [
            'additional-services',
            'all-services',
        ];

        // Sort services by category order (if 'all'), then by price within category
        return filtered.sort((a, b) => {
            if (activeCategory === 'all') {
                // Get category priorities
                const startIndexA = startPriority.indexOf(a.category);
                const startIndexB = startPriority.indexOf(b.category);
                const endIndexA = endPriority.indexOf(a.category);
                const endIndexB = endPriority.indexOf(b.category);

                // Compare categories first
                // Both in start priority
                if (startIndexA !== -1 && startIndexB !== -1) {
                    if (startIndexA !== startIndexB) return startIndexA - startIndexB;
                }
                // Only a is in start priority
                else if (startIndexA !== -1) return -1;
                // Only b is in start priority
                else if (startIndexB !== -1) return 1;

                // Both in end priority
                if (endIndexA !== -1 && endIndexB !== -1) {
                    if (endIndexA !== endIndexB) return endIndexA - endIndexB;
                }
                // Only a is in end priority
                else if (endIndexA !== -1) return 1;
                // Only b is in end priority
                else if (endIndexB !== -1) return -1;

                // Middle categories - sort alphabetically
                if (a.category !== b.category) return a.category.localeCompare(b.category);
            }

            // Within same category, sort by price (low to high)
            return a.price - b.price;
        });
    };

    const handleContinue = () => {
        if (getSelectedServicesCount() === 0) {
            setError('Please add at least one service to continue');
            window.scrollTo({ top: 0, behavior: 'smooth' });
            return;
        }

        const selectedServicesData = services.filter(s => selectedServices[s.id]);
        const servicesWithQuantities = selectedServicesData.map(service => ({
            ...service,
            quantity: selectedServices[service.id]
        }));

        updateBookingData({
            selectedServices,
            selectedServicesData: servicesWithQuantities,
            totalAmount: calculateTotal(),
            totalDuration: calculateTotalDuration()
        });
        navigate('/time-slots');
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center min-h-[60vh]">
                <div className="text-center">
                    <div
                        className="animate-spin rounded-full h-16 w-16 border-4 border-t-transparent mx-auto"
                        style={{ borderColor: '#46C6CE', borderTopColor: 'transparent' }}
                    ></div>
                    <p className="mt-6 text-gray-600 font-medium text-lg">Loading services...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-gray-50 via-white to-blue-50/30 py-8 px-4">
            <div className="max-w-7xl mx-auto">


                {/* Error Message */}
                {error && (
                    <div className="max-w-4xl mx-auto bg-red-50 border-l-4 border-red-500 rounded-lg p-4 mb-6 animate-in fade-in slide-in-from-top duration-300">
                        <div className="flex items-center">
                            <svg className="h-5 w-5 text-red-500 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                            <p className="text-red-700 font-semibold">{error}</p>
                        </div>
                    </div>
                )}

                {/* Discount Promotion Banner */}
                <div className="max-w-4xl mx-auto mb-6">
                    <div className="bg-gradient-to-r from-green-50 to-emerald-50 border-2 border-green-300 rounded-xl p-4 shadow-md">
                        <div className="flex items-center justify-center gap-3">
                            <div className="bg-green-500 rounded-full p-2">

                            </div>
                            <div className="text-center">
                                <p className="text-green-800 font-bold text-lg">
                                    Multi-Service Discount!
                                </p>
                                <p className="text-green-700 text-sm mt-1">
                                    2nd service <span className="font-bold">10% OFF</span> • 3rd+ services <span className="font-bold">20% OFF</span>
                                </p>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Category Filter */}
                <div className="flex flex-wrap gap-3 justify-center mb-8">
                    {categories.map(category => (
                        <button
                            key={category.id}
                            onClick={() => setActiveCategory(category.id)}
                            className={`
                                flex items-center gap-2 px-4 py-2.5 rounded-xl border-2 transition-all duration-300
                                ${activeCategory === category.id
                                    ? 'shadow-md transform scale-105'
                                    : 'hover:shadow-sm hover:-translate-y-0.5'
                                }
                            `}
                            style={{
                                backgroundColor: activeCategory === category.id ? category.color : 'white',
                                borderColor: activeCategory === category.id ? category.color : '#E5E7EB',
                                color: activeCategory === category.id ? 'white' : '#4B5563'
                            }}
                        >
                            <span className="text-lg">{category.icon}</span>
                            <span className="font-medium">{category.name}</span>
                        </button>
                    ))}
                </div>

                {/* Services Grid - Smaller Cards */}
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 mb-8 max-w-6xl mx-auto">
                    {getFilteredServices().map(service => {
                        const quantity = selectedServices[service.id] || 0;
                        const isSelected = quantity > 0;

                        return (
                            <div
                                key={service.id}
                                className={`
                                    relative bg-white rounded-xl border p-4 transition-all duration-300 h-full
                                    ${isSelected
                                        ? 'border-[#194376] shadow-lg transform scale-102'
                                        : 'border-gray-200 hover:border-[#46C6CE]/50 hover:shadow-md'
                                    }
                                `}
                            >
                                {/* Popular Badge */}
                                {service.popular && (
                                    <div
                                        className="absolute -top-2 -right-2 px-2 py-1 rounded-full text-xs font-bold shadow-md z-10"
                                        style={{ backgroundColor: '#46C6CE', color: 'white' }}
                                    >
                                        Popular
                                    </div>
                                )}

                                {/* Service Content */}
                                <div className="space-y-3">
                                    {/* Category Badge */}
                                    <div className="flex justify-between items-start">
                                        <span className="text-xs font-medium px-2 py-1 rounded bg-gray-100 text-gray-600">
                                            {service.category.replace('-', ' ')}
                                        </span>
                                        {isSelected && (
                                            <span className="text-xs font-bold px-2 py-1 rounded bg-[#46C6CE] text-white">
                                                {quantity} selected
                                            </span>
                                        )}
                                    </div>

                                    {/* Service Info */}
                                    <div>
                                        <h3 className="font-bold text-gray-800 text-sm mb-1 line-clamp-2">
                                            {service.name}
                                        </h3>
                                        <p className="text-gray-600 text-xs mb-3 line-clamp-2">
                                            {service.description}
                                        </p>
                                    </div>

                                    {/* Duration & Price */}
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center text-gray-600 text-xs">
                                            <svg className="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                                            </svg>
                                            <span>{service.duration} min</span>
                                        </div>
                                        <div className="text-right">
                                            {(() => {
                                                // Calculate what position this service would be if added
                                                const currentUnitCount = Object.values(selectedServices).reduce((sum, qty) => sum + qty, 0);
                                                const wouldBePosition = isSelected ? -1 : currentUnitCount; // -1 means already selected
                                                const discountRate = wouldBePosition === 0 ? 0 : wouldBePosition === 1 ? 0.10 : wouldBePosition >= 2 ? 0.20 : 0;
                                                const discountedPrice = service.price * (1 - discountRate);
                                                const hasDiscount = discountRate > 0 && !isSelected;

                                                return (
                                                    <>
                                                        {hasDiscount ? (
                                                            <div>
                                                                <div className="text-xs text-gray-400 line-through">£{service.price.toFixed(2)}</div>
                                                                <div className="font-bold text-green-600">£{discountedPrice.toFixed(2)}</div>
                                                                <div className="text-xs bg-green-100 text-green-700 px-1 py-0.5 rounded font-bold inline-block">
                                                                    -{(discountRate * 100).toFixed(0)}% off
                                                                </div>
                                                            </div>
                                                        ) : (
                                                            <>
                                                                <div className="font-bold text-gray-800">£{service.price.toFixed(2)}</div>
                                                                <div className="text-xs text-gray-500">per service</div>
                                                            </>
                                                        )}
                                                    </>
                                                );
                                            })()}
                                        </div>
                                    </div>

                                    {/* Quantity Controls */}
                                    <div className="pt-2 border-t border-gray-100">
                                        {isSelected ? (
                                            <div className="flex items-center justify-between">
                                                <button
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        handleQuantityChange(service.id, -1);
                                                    }}
                                                    className="w-8 h-8 rounded-full border border-gray-300 flex items-center justify-center hover:bg-gray-100 active:bg-gray-200 transition-colors"
                                                >
                                                    <span className="text-lg font-bold text-gray-600">−</span>
                                                </button>

                                                <div className="text-center">
                                                    <div className="font-bold text-gray-800 text-lg">{quantity}</div>
                                                    <div className="text-xs text-gray-500">selected</div>
                                                </div>

                                                <button
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        handleQuantityChange(service.id, 1);
                                                    }}
                                                    className="w-8 h-8 rounded-full flex items-center justify-center text-white hover:opacity-90 active:scale-95 transition-all"
                                                    style={{ backgroundColor: '#46C6CE' }}
                                                >
                                                    <span className="text-lg font-bold">+</span>
                                                </button>
                                            </div>
                                        ) : (
                                            <button
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    handleAddService(service.id);
                                                }}
                                                className="w-full py-2 rounded-lg font-medium text-sm transition-all hover:scale-102 active:scale-98"
                                                style={{
                                                    backgroundColor: '#46C6CE',
                                                    color: 'white'
                                                }}
                                            >
                                                Add to Booking
                                            </button>
                                        )}
                                    </div>
                                </div>
                            </div>
                        );
                    })}
                </div>

                {/* Selected Services Summary - Fixed at bottom on mobile */}
                {getSelectedServicesCount() > 0 && (
                    <div className="sticky bottom-0 bg-white border-t-2 border-gray-200 shadow-2xl rounded-t-2xl p-6 mb-8 z-50">
                        <div className="max-w-4xl mx-auto">
                            <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-6">
                                {/* Summary Info */}
                                <div className="flex-1">
                                    <div className="flex items-center mb-2">
                                        <div
                                            className="w-10 h-10 rounded-lg flex items-center justify-center mr-3"
                                            style={{ background: '#194376' }}
                                        >
                                            <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
                                            </svg>
                                        </div>
                                        <div>
                                            <h3 className="font-bold text-gray-800 text-lg">Booking Summary</h3>
                                            <p className="text-gray-600 text-sm">
                                                {getSelectedServicesCount()} service{getSelectedServicesCount() !== 1 ? 's' : ''} • {calculateTotalDuration()} minutes
                                            </p>
                                        </div>
                                    </div>
                                </div>

                                {/* Totals */}
                                <div className="flex flex-col sm:flex-row items-center gap-4">
                                    <div className="text-center sm:text-right">
                                        <div className="text-2xl font-bold text-[#194376]">
                                            £{calculateTotal().toFixed(2)}
                                        </div>
                                        <div className="text-sm text-gray-500">Total Amount</div>
                                    </div>

                                    {/* Action Buttons */}
                                    <div className="flex gap-3">
                                        <button
                                            onClick={() => navigate('/postcode')}
                                            className="px-6 py-3 border-2 border-gray-300 text-gray-700 font-bold rounded-xl hover:bg-gray-50 hover:border-gray-400 transition-all flex items-center gap-2"
                                        >
                                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 19l-7-7 7-7" />
                                            </svg>
                                            Back
                                        </button>

                                        <button
                                            onClick={handleContinue}
                                            className="px-8 py-3 font-bold rounded-xl transition-all text-white hover:shadow-2xl transform hover:scale-105 active:scale-95 flex items-center gap-2"
                                            style={{ background: '#194376' }}
                                        >
                                            Continue to Time Slots
                                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M13 7l5 5m0 0l-5 5m5-5H6" />
                                            </svg>
                                        </button>
                                    </div>
                                </div>
                            </div>

                            {/* Selected Services List - Collapsible */}
                            <div className="mt-4 pt-4 border-t border-gray-100">
                                <div className="flex flex-wrap gap-2">
                                    {(() => {
                                        let unitCounterForSummary = 0;
                                        return services
                                            .filter(s => selectedServices[s.id])
                                            .map((service) => {
                                                const quantity = selectedServices[service.id];
                                                const originalPriceTotal = service.price * quantity;
                                                let discountedPriceTotal = 0;

                                                for (let i = 0; i < quantity; i++) {
                                                    const unitDiscountRate = unitCounterForSummary === 0 ? 0 : unitCounterForSummary === 1 ? 0.10 : 0.20;
                                                    discountedPriceTotal += service.price * (1 - unitDiscountRate);
                                                    unitCounterForSummary++;
                                                }

                                                const averageDiscountRate = (originalPriceTotal - discountedPriceTotal) / originalPriceTotal;
                                                const hasDiscount = averageDiscountRate > 0;

                                                return (
                                                    <div
                                                        key={service.id}
                                                        className="flex items-center gap-2 bg-gray-50 rounded-lg px-3 py-2"
                                                    >
                                                        <span className="font-medium text-sm text-gray-800">{service.name}</span>
                                                        <div className="flex items-center gap-1">
                                                            <span className="text-xs bg-white px-1.5 py-0.5 rounded font-bold">
                                                                {quantity}×
                                                            </span>
                                                            {hasDiscount ? (
                                                                <div className="flex items-center gap-1">
                                                                    <span className="text-xs text-gray-400 line-through">
                                                                        £{originalPriceTotal.toFixed(2)}
                                                                    </span>
                                                                    <span className="text-sm text-green-600 font-bold">
                                                                        £{discountedPriceTotal.toFixed(2)}
                                                                    </span>
                                                                    <span className="text-xs bg-green-100 text-green-700 px-1 py-0.5 rounded font-bold">
                                                                        -{(averageDiscountRate * 100).toFixed(0)}% avg
                                                                    </span>
                                                                </div>
                                                            ) : (
                                                                <span className="text-sm text-gray-600">£{originalPriceTotal.toFixed(2)}</span>
                                                            )}
                                                        </div>
                                                    </div>
                                                );
                                            });
                                    })()}
                                </div>
                            </div>
                        </div>
                    </div>
                )}

            </div>
        </div>
    );
};

export default ServicesPage;
