// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// --- Back to Top ---
const btnTop = document.getElementById("back-top");

if (btnTop) {
    window.addEventListener("scroll", function () {
        if (window.scrollY > 100) {
            btnTop.style.display = "flex";
        } else {
            btnTop.style.display = "none";
        }
    });

    btnTop.addEventListener("click", function () {
        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    });
}