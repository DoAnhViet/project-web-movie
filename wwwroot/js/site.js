// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Draggable horizontal slider for elements with class `halim-trending-track`.
(function () {
	function makeDraggable(container) {
		let isDown = false;
		let startX;
		let scrollLeft;

		container.addEventListener('mousedown', (e) => {
			isDown = true;
			container.classList.add('active');
			startX = e.pageX - container.offsetLeft;
			scrollLeft = container.scrollLeft;
			e.preventDefault();
		});

		container.addEventListener('mouseleave', () => {
			isDown = false;
			container.classList.remove('active');
		});

		container.addEventListener('mouseup', () => {
			isDown = false;
			container.classList.remove('active');
		});

		container.addEventListener('mousemove', (e) => {
			if (!isDown) return;
			e.preventDefault();
			const x = e.pageX - container.offsetLeft;
			const walk = (x - startX) * 1.2; // scroll-fast
			container.scrollLeft = scrollLeft - walk;
		});

		// Touch support
		container.addEventListener('touchstart', (e) => {
			startX = e.touches[0].pageX - container.offsetLeft;
			scrollLeft = container.scrollLeft;
		}, { passive: true });

		container.addEventListener('touchmove', (e) => {
			const x = e.touches[0].pageX - container.offsetLeft;
			const walk = (x - startX) * 1.2;
			container.scrollLeft = scrollLeft - walk;
		}, { passive: true });
	}

	document.addEventListener('DOMContentLoaded', function () {
		document.querySelectorAll('.halim-trending-track').forEach(makeDraggable);
	});
})();
