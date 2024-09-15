import * as React from 'react';
import { Row, Container, Spinner, Col, Alert } from 'react-bootstrap';
import { completeReservation } from '../api/listingsApi';
import { navigateToPage } from '../api/navigation';

const CheckoutComplete: React.FC = () => {
    async function process() {
        // Get reservationId from query string parameter 'r'
        const urlParams = new URLSearchParams(window.location.search);
        const reservationId = urlParams.get('id') ?? '';
        await completeReservation(reservationId);
        // Redirect to the profile page
        const currentDate = new Date();
        currentDate.setDate(currentDate.getDate() + 1);
        // Set the time to midnight to ensure the reservation is shown
        currentDate.setHours(0, 0, 0, 0);
        const formattedDate = currentDate.toISOString();
        // Adding the custom metadata filter to show the reservation that was just completed
        // This ensures that the output cache is invalidated and the reservation is shown
        navigateToPage(`/profile/?mf=0=${formattedDate}`);
    }

    React.useEffect(() => {
        process();
    }, []);

    return (
        <>
            <Container fluid>
                <Row>
                    <Col className="d-flex flex-column align-items-center">
                        <Alert variant="success" className="p-4">
                            <Alert.Heading>
                                <Spinner
                                    as="span"
                                    animation="border"
                                    role="status"
                                    aria-hidden="true"
                                    className="mx-2"
                                ></Spinner>
                                Please wait while we complete your reservation!
                            </Alert.Heading>
                        </Alert>
                    </Col>
                </Row>
            </Container>
        </>
    );
};

export default CheckoutComplete;
