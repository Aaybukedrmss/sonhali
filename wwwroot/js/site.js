// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
    window.__API_BASE__ = window.__API_BASE__ || "http://localhost:5090/api";
    const menuToggle = document.getElementById("menuToggle");
    const dropdownMenu = document.getElementById("dropdownMenu");

    if (!menuToggle || !dropdownMenu) return;

    menuToggle.addEventListener("click", function (e) {
        e.stopPropagation();
        dropdownMenu.classList.toggle("active");
    });

    document.addEventListener("click", function (event) {
        if (!menuToggle.contains(event.target) && !dropdownMenu.contains(event.target)) {
            dropdownMenu.classList.remove("active");
        }
    });

    // AJAX favorite toggle for detail main and similar products
    document.querySelectorAll('.btn-fav-toggle').forEach(function(btn){
        btn.addEventListener('click', async function(){
            const urunId = this.getAttribute('data-urun-id');
            try {
                const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                const token = tokenInput ? tokenInput.value : '';
                const res = await fetch('/Favorite/ToggleAjax', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded'
                    },
                    body: `urunId=${encodeURIComponent(urunId)}&__RequestVerificationToken=${encodeURIComponent(token)}`
                });
                if(!res.ok){
                    if(res.status === 401){
                        window.location.href = '/Account/Login';
                        return;
                    }
                    throw new Error('Favori işlemi başarısız');
                }
                const data = await res.json();
                const icon = this.querySelector('i');
                const label = this.querySelector('.fav-label');
                if(data.isFavorite){
                    icon.classList.remove('far','text-muted');
                    icon.classList.add('fas','text-danger');
                    this.classList.add('favorite-active');
                    if(label){ label.textContent = this.getAttribute('data-label-fav') || 'Favorilerde'; }
                } else {
                    icon.classList.remove('fas','text-danger');
                    icon.classList.add('far','text-muted');
                    this.classList.remove('favorite-active');
                    if(label){ label.textContent = this.getAttribute('data-label-not') || 'Favorilere Ekle'; }
                }
            } catch(err){
                console.error(err);
            }
        });
    });
});
