// KirazCo E-Ticaret - JavaScript Dosyası
console.log('Site.js yüklendi');

// Sayfa yüklendiğinde çalışacak fonksiyonlar
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOMContentLoaded event çalışıyor');
    
    // Ürün detay sayfası favorileme butonu
    initProductDetailFavorite();
    
    // Ürün kartları favorileme butonu
    initProductCardFavorites();
    
    // Menu toggle sistemi
    initMenuToggle();
});

// Alternatif olarak sayfa yüklendiğinde de çalıştır
window.addEventListener('load', function() {
    console.log('Window load event çalışıyor');
    
    // Ürün detay sayfası favorileme butonu
    initProductDetailFavorite();
    
    // Ürün kartları favorileme butonu
    initProductCardFavorites();
    
    // Menu toggle sistemi
    initMenuToggle();
});

// Ürün detay sayfası favorileme sistemi
function initProductDetailFavorite() {
    console.log('initProductDetailFavorite fonksiyonu çalışıyor...');
    
    const favoriteButton = document.querySelector('.product-detail-fav-toggle');
    
    if (!favoriteButton) {
        console.log('Ürün detay favorileme butonu bulunamadı');
        return;
    }
    
    console.log('Ürün detay favorileme butonu bulundu:', favoriteButton);
    
    // Event listener ekle
    favoriteButton.addEventListener('click', handleFavoriteClick);
    console.log('Event listener eklendi');
}

// Ürün kartları favorileme sistemi
function initProductCardFavorites() {
    console.log('initProductCardFavorites fonksiyonu çalışıyor...');
    
    const favoriteButtons = document.querySelectorAll('.product-card-fav-toggle');
    console.log(`${favoriteButtons.length} ürün kartı favorileme butonu bulundu`);
    
    if (favoriteButtons.length === 0) {
        console.log('Hiç ürün kartı favorileme butonu bulunamadı');
        return;
    }
    
    favoriteButtons.forEach((button, index) => {
        console.log(`Buton ${index + 1}:`, button);
        
        // Event listener ekle
        button.addEventListener('click', handleFavoriteClick);
        console.log(`Buton ${index + 1} için event listener eklendi`);
    });
    
    console.log('Tüm ürün kartı favorileme butonlarına event listener eklendi');
}

// Birleşik favorileme buton tıklama işlemi
function handleFavoriteClick(event) {
    console.log('handleFavoriteClick fonksiyonu çalışıyor...');
    
    event.preventDefault();
    event.stopPropagation();
    
    const button = event.currentTarget;
    const urunId = button.getAttribute('data-urun-id');
    
    console.log('Tıklanan buton:', button);
    console.log('UrunId:', urunId);
    
    if (!urunId) {
        console.error('UrunId bulunamadı!');
        return;
    }
    
    // Buton tipini belirle (ürün kartı mı detay mı)
    const isProductCard = button.classList.contains('product-card-fav-toggle');
    const isProductDetail = button.classList.contains('product-detail-fav-toggle');
    
    console.log('Buton tipi:', isProductCard ? 'Ürün Kartı' : 'Ürün Detay');
    
    // Butonu devre dışı bırak ve loading state'i ayarla
    button.disabled = true;
    const originalHTML = button.innerHTML;
    
    if (isProductCard) {
        // Ürün kartı için sadece spinner
        button.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
    } else {
        // Ürün detay için spinner + text
        button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i><span>İşleniyor...</span>';
    }
    
    // Token'ı bul
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) {
        console.error('Anti-forgery token bulunamadı!');
        button.disabled = false;
        button.innerHTML = originalHTML;
        return;
    }
    
    const token = tokenInput.value;
    
    // AJAX isteği - timeout ile
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 5000); // 5 saniye timeout
    
    fetch('/Favorite/ToggleAjax', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: `urunId=${encodeURIComponent(urunId)}&__RequestVerificationToken=${encodeURIComponent(token)}`,
        signal: controller.signal
    })
    .then(response => {
        clearTimeout(timeoutId); // Timeout'u temizle
        if (!response.ok) {
            if (response.status === 401) {
                showNotification('Favorilere eklemek için giriş yapmalısınız!', 'error');
                setTimeout(() => {
                    window.location.href = '/Account/Login';
                }, 2000);
                return;
            }
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        return response.json();
    })
    .then(data => {
        console.log('Sunucu yanıtı:', data);
        
        if (!data.success) {
            throw new Error(data.error || 'Bilinmeyen hata');
        }
        
        // Butonu güncelle
        if (data.isFavorite) {
            // Favoriye eklendi
            button.classList.add('favorite-active');
            
            if (isProductCard) {
                // Ürün kartı için sadece class değişimi
                button.innerHTML = '<i class="fa-solid fa-heart"></i>';
            } else {
                // Ürün detay için icon ve text güncelleme
                const icon = button.querySelector('i');
                const span = button.querySelector('span');
                if (icon) icon.className = 'fas fa-heart me-2';
                if (span) span.textContent = 'Favorilerde';
            }
            
            showNotification('❤️ Ürün favorilere eklendi!', 'success');
        } else {
            // Favoriden çıkarıldı
            button.classList.remove('favorite-active');
            
            if (isProductCard) {
                // Ürün kartı için sadece class değişimi
                button.innerHTML = '<i class="fa-solid fa-heart"></i>';
            } else {
                // Ürün detay için icon ve text güncelleme
                const icon = button.querySelector('i');
                const span = button.querySelector('span');
                if (icon) icon.className = 'far fa-heart me-2';
                if (span) span.textContent = 'Favorilere Ekle';
            }
            
            showNotification('💔 Ürün favorilerden çıkarıldı!', 'info');
        }
        
        // Butonu tekrar aktif et
        button.disabled = false;
    })
    .catch(error => {
        clearTimeout(timeoutId); // Timeout'u temizle
        console.error('Favori işlemi hatası:', error);
        
        if (error.name === 'AbortError') {
            showNotification('⏱️ İşlem zaman aşımına uğradı!', 'error');
        } else {
            showNotification('❌ Hata oluştu!', 'error');
        }
        
        // Butonu tekrar aktif et
        button.disabled = false;
        button.innerHTML = originalHTML;
    });
}

// Bildirim gösterme fonksiyonu
let lastNotification = 0;

function showNotification(message, type) {
    const now = Date.now();
    
    // 3 saniye içinde tekrar bildirim gösterme
    if (now - lastNotification < 3000) {
        return;
    }
    
    lastNotification = now;
    
    // Mevcut bildirimleri kaldır
    const existing = document.querySelectorAll('.favorite-notification');
    existing.forEach(n => n.remove());
    
    // Yeni bildirim oluştur
    const notification = document.createElement('div');
    notification.className = 'favorite-notification';
    notification.textContent = message;
    
    // Tip'e göre stil belirle
    let bgColor, textColor;
    switch(type) {
        case 'success':
            bgColor = '#f6a7a1';
            textColor = 'white';
            break;
        case 'info':
            bgColor = '#f7c7ba';
            textColor = '#3c2323';
            break;
        case 'error':
            bgColor = '#dc3545';
            textColor = 'white';
            break;
        default:
            bgColor = '#f6a7a1';
            textColor = 'white';
    }
    
    // Stil
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background-color: ${bgColor};
        color: ${textColor};
        padding: 15px 20px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.3);
        z-index: 9999;
        font-weight: 500;
        font-size: 14px;
        max-width: 300px;
        animation: slideInRight 0.3s ease-out;
    `;
    
    // Animasyon CSS'i ekle
    if (!document.querySelector('#notification-styles')) {
        const style = document.createElement('style');
        style.id = 'notification-styles';
        style.textContent = `
            @keyframes slideInRight {
                from {
                    transform: translateX(100%);
                    opacity: 0;
                }
                to {
                    transform: translateX(0);
                    opacity: 1;
                }
            }
            @keyframes slideOutRight {
                from {
                    transform: translateX(0);
                    opacity: 1;
                }
                to {
                    transform: translateX(100%);
                    opacity: 0;
                }
            }
        `;
        document.head.appendChild(style);
    }
    
    // Sayfaya ekle
    document.body.appendChild(notification);
    
    // 3 saniye sonra kaldır
    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.3s ease-out';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }, 3000);
}

// Menu toggle sistemi
function initMenuToggle() {
    const menuToggle = document.getElementById("menuToggle");
    const dropdownMenu = document.getElementById("dropdownMenu");

    if (menuToggle && dropdownMenu) {
        menuToggle.addEventListener("click", function (e) {
            e.stopPropagation();
            dropdownMenu.classList.toggle("active");
        });

        document.addEventListener("click", function (event) {
            if (!menuToggle.contains(event.target) && !dropdownMenu.contains(event.target)) {
                dropdownMenu.classList.remove("active");
            }
        });
    }
}

console.log('=== SITE.JS TAMAMLANDI ===');