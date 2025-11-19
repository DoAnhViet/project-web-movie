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

	// History page: live search filter and delete handler
	document.addEventListener('DOMContentLoaded', function () {
		// Live filter for history cards
		var searchInput = document.getElementById('history-search');
		if (searchInput) {
			searchInput.addEventListener('input', function (e) {
				var q = (e.target.value || '').toLowerCase().trim();
				document.querySelectorAll('.history-card').forEach(function (card) {
					var titleEl = card.querySelector('.history-info-title');
					var title = titleEl ? (titleEl.textContent || '').toLowerCase() : '';
					if (!q || title.indexOf(q) !== -1) {
						card.style.display = '';
					} else {
						card.style.display = 'none';
					}
				});
			});
		}

		// Delete history item (delegated)
		document.addEventListener('click', function (ev) {
			var del = ev.target.closest ? ev.target.closest('.delete-history') : null;
			if (!del) return;
			var id = del.getAttribute('data-history-id');
			if (!id) return;
			if (!confirm('Bạn có chắc muốn xóa mục lịch sử này?')) return;

			fetch('/Watch/DeleteHistory', {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json'
				},
				body: JSON.stringify({ id: parseInt(id, 10) })
			})
				.then(function (res) { return res.json(); })
				.then(function (json) {
if (json && json.success) {
						var card = document.querySelector('.history-card[data-history-id="' + id + '"]');
						if (card) card.remove();
					} else {
						alert(json && json.message ? json.message : 'Xóa thất bại');
					}
				})
				.catch(function () { alert('Lỗi khi xóa lịch sử'); });
		});
	});