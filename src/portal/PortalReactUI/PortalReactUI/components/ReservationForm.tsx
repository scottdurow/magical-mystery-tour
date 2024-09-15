import * as React from 'react';
import { Row, Col, Form, Alert, Button, Spinner } from 'react-bootstrap';
import DatePicker from 'react-datepicker';
import { BsCartPlus } from 'react-icons/bs';
import * as ReservationApi from '../api/listingsApi';
import { ApiError } from '../api/apiErrors';
import { navigateToPage } from '../api/navigation';

interface ReservationFormProps {
    listingId: string;
}

const ReservationForm: React.FC<ReservationFormProps> = ({ listingId }) => {
    const [isReserving, setIsReserving] = React.useState(false);
    const [error, setError] = React.useState<string | null>(null);
    const [startDate, setStartDate] = React.useState<Date | null>();
    const [endDate, setEndDate] = React.useState<Date | null>();
    const [guests, setGuests] = React.useState<number>(1);

    async function reserve() {
        // Reserve the listing
        if (listingId && startDate && endDate) {
            try {
                setIsReserving(true);
                setError(null); // Clear any previous errors
                // Call the reserve listing API
                // Get the dates/guests from the page
                const response = await ReservationApi.reserveListing(listingId, startDate, endDate, guests);
                // Redirect to the checkout page
                navigateToPage(response.sessionUrl);
            } catch (err) {
                console.error('Error reserving listing:', err);
                const errorMessage =
                    err instanceof ApiError ? err.message : 'An error occurred while reserving the listing.';

                setError(errorMessage);
            } finally {
                setIsReserving(false);
            }
        }
    }
    return (
        <>
            <Row className="mb-3">
                <Col xs={4} className="text-right">
                    <label htmlFor="startDate">Start Date:</label>
                </Col>
                <Col xs={8}>
                    <DatePicker
                        id="startDate"
                        selected={startDate}
                        onChange={(date: Date | null) => setStartDate(date)}
                        selectsStart
                        startDate={startDate ?? undefined}
                        endDate={endDate}
                        showIcon
                        icon
                        className="form-control"
                    />
                </Col>
            </Row>
            <Row className="mb-3">
                <Col xs={4} className="text-right">
                    <label htmlFor="endDate">End Date:</label>
                </Col>
                <Col xs={8}>
                    <DatePicker
                        id="endDate"
                        selected={endDate}
                        onChange={(date: Date | null) => setEndDate(date)}
                        selectsEnd
                        startDate={startDate ?? undefined}
                        endDate={endDate}
                        minDate={startDate}
                        showIcon
                        icon
                        className="form-control"
                    />
                </Col>
            </Row>
            <Row className="mb-3">
                <Col xs={4} className="text-right">
                    <label htmlFor="guests">Number of Guests:</label>
                </Col>
                <Col xs={2}>
                    <Form.Control
                        type="number"
                        id="guests"
                        value={guests}
                        onChange={(e: React.ChangeEvent<HTMLInputElement>) => setGuests(Number(e.target.value))}
                        min={1}
                        className="form-control"
                    />
                </Col>
            </Row>
            {error && <Alert variant="danger">{error}</Alert>}
            <Button
                onClick={reserve}
                className="bg-light-primary"
                style={{ borderRadius: '0', border: 0 }}
                disabled={isReserving || !startDate || !endDate}
            >
                {isReserving ? (
                    <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                ) : (
                    <>
                        <BsCartPlus size="1.8rem" />
                        RESERVE
                    </>
                )}
            </Button>
        </>
    );
};

export default ReservationForm;
