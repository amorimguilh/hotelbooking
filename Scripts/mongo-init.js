print('Start #################################################################');
db.createCollection('reservations');
db.collection.createIndex({ "startDate": 1, "endDate": 1  },{ unique: true });