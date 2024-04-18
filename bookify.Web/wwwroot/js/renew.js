$(document).ready(function () {
	$('.js-renew').on('click', function () {
		var subscriberKey = $(this).data('key');
		bootbox.confirm({
			message: 'are you sure that you need renew subscribtion?',
			buttons: {
				confirm: {
					label: 'Yes',
					className: 'btn-danger'
				},
				cancel: {
					label: 'No',
					className: 'btn-secondary'
				}
			},
			callback: function (result) {
				if (result) {
					$.post({
						url: `/Subscribers/RenewSubscription?sKey=${subscriberKey}`,
						data: {
							'__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
						},
						success: function (row) {
							$('#susbscriptionTable').find('tbody').append(row);
							var activeIcon = $('#ActiveStatusIcon');
							activeIcon.removeClass('d-none');
							activeIcon.siblings('svg').remove();
							activeIcon.parents('.card').removeClass('bg-warning').addClass('bg-success');
							$('#CardStatus').text('Active subscriber');
							$('#StatusBage').removeClass('badge-light-warning').addClass('badge-light-success').text('Active subscriber');

							showSuccessMessage();
						},
						error: function () {
							showErrorMessage();
						}
					});
				}
			}
		});
    });
});