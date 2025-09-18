// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

console.log('=== SITE.JS YÜKLENDİ ===');

// Basit ve güvenilir favorileme sistemi
document.addEventListener('DOMContentLoaded', function() {
    console.log('=== DOMContentLoaded ÇALIŞTI ===');
    
    // Favori butonlarını bul ve event listener ekle
    function initFavoriteButtons() {
        console.log('=== FAVORİ BUTONLARI BAŞLATILIYOR ===');
        
        // Tüm butonları bul
        const allButtons = document.querySelectorAll('button');
        console.log(`Toplam ${allButtons.length} buton bulundu`);
        
        // Favori butonlarını bul
        const favoriteButtons = document.querySelectorAll('.btn-fav-toggle');
        console.log(`${favoriteButtons.length} favori butonu bulundu`);
        
        // Eğer favori butonu yoksa, tüm butonları kontrol et
        if (favoriteButtons.length === 0) {
            console.log('Favori butonu bulunamadı! Tüm butonları kontrol ediyorum...');
            allButtons.forEach((btn, index) => {
                console.log(`Buton ${index + 1}:`, {
                    classes: btn.className,
                    innerHTML: btn.innerHTML,
                    dataUrunId: btn.getAttribute('data-urun-id')
                });
            });
        }
        
        favoriteButtons.forEach((button, index) => {
            const urunId = button.getAttribute('data-urun-id');
            console.log(`Favori Buton ${index + 1}: UrunId = ${urunId}`);
            console.log('Buton HTML:', button.outerHTML);
            
            button.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                
                console.log(`=== FAVORİ BUTONU TIKLANDI: UrunId = ${urunId} ===`);
                toggleFavorite(button, urunId);
            });
        });
        
        console.log('=== FAVORİ BUTONLARI BAŞLATMA TAMAMLANDI ===');
    }
    
    // Favori toggle işlemi
    async function toggleFavorite(button, urunId) {
        if (!urunId) {
            console.error('UrunId bulunamadı');
            return;
        }
        
        // Butonu devre dışı bırak
        const originalHTML = button.innerHTML;
        button.disabled = true;
        button.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
        
        try {
            // Token'ı bul
            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            if (!tokenInput) {
                throw new Error('Anti-forgery token bulunamadı');
            }
            
            const token = tokenInput.value;
            console.log('Token bulundu, AJAX isteği gönderiliyor...');
            
            // AJAX isteği
            const response = await fetch('/Favorite/ToggleAjax', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: `urunId=${encodeURIComponent(urunId)}&__RequestVerificationToken=${encodeURIComponent(token)}`
            });
            
            console.log('Response status:', response.status);
            
            if (!response.ok) {
                if (response.status === 401) {
                    alert('Favorilere eklemek için giriş yapmalısınız!');
                    window.location.href = '/Account/Login';
                    return;
                }
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            
            const data = await response.json();
            console.log('Response data:', data);
            
            // Butonu güncelle
            const icon = button.querySelector('i');
            const span = button.querySelector('span');
            
            if (data.isFavorite) {
                // Favoriye eklendi
                if (icon) {
                    icon.className = 'fas fa-heart';
                }
                button.classList.add('favorite-active');
                if (span) span.textContent = 'Favorilerde';
                showNotification('❤️ Ürün favorilere eklendi!', 'success');
            } else {
                // Favoriden çıkarıldı
                if (icon) {
                    icon.className = 'far fa-heart';
                }
                button.classList.remove('favorite-active');
                if (span) span.textContent = 'Favorilere Ekle';
                showNotification('💔 Ürün favorilerden çıkarıldı!', 'info');
            }
            
        } catch (error) {
            console.error('Favori işlemi hatası:', error);
            showNotification('❌ Favori işlemi sırasında hata oluştu!', 'error');
        } finally {
            // Butonu tekrar aktif et
            button.disabled = false;
            if (button.innerHTML.includes('fa-spinner')) {
                button.innerHTML = originalHTML;
            }
        }
    }
    
    // Bildirim gösterme fonksiyonu
    function showNotification(message, type) {
        // Mevcut bildirimleri kaldır
        const existingNotifications = document.querySelectorAll('.favorite-notification');
        existingNotifications.forEach(notification => notification.remove());
        
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
                bgColor = '#f7c7ba';
                textColor = '#3c2323';
                break;
            default:
                bgColor = '#f6a7a1';
                textColor = 'white';
        }
        
        // Stil uygula
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background-color: ${bgColor};
            color: ${textColor};
            padding: 15px 20px;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(246, 167, 161, 0.3);
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
    
    // Favori butonlarını başlat
    initFavoriteButtons();
    
    // Sayfa değiştiğinde tekrar başlat (AJAX sayfa değişiklikleri için)
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                // Yeni favori butonları eklenmiş olabilir
                setTimeout(initFavoriteButtons, 100);
            }
        });
    });
    
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
    
    // Menu toggle sistemi
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
    
    console.log('Tüm sistemler başlatıldı');
});